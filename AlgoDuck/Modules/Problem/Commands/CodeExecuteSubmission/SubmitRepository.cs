using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Item.Utils;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

public interface IExecutorSubmitRepository
{
    public Task<Result<bool, ErrorObject<string>>> InsertSubmissionResultAsync(SubmissionInsertDto insertDto, CancellationToken cancellationToken = default);
    // private Task<Result<bool, ErrorObject<string>>> DropLastCheckpointAsync(AutoSaveDropDto dropDto, CancellationToken cancellationToken = default)

    public Task<Result<RewardsDto, ErrorObject<string>>> AddCoinsAndExperienceAsync(SolutionRewardDto rewardDto, CancellationToken cancellationToken = default);
}

public class SubmitRepository(
    ApplicationCommandDbContext commandDbContext,
    IAwsS3Client awsS3Client,
    IOptions<AwardsConfig> awardsConfig
    ) : IExecutorSubmitRepository
{

    public async Task<Result<bool, ErrorObject<string>>> InsertSubmissionResultAsync(SubmissionInsertDto insertDto, CancellationToken cancellationToken = default)
    {
        var solution = new UserSolution
        {
            CodeRuntimeSubmitted = insertDto.CodeRuntimeSubmitted,
            ProblemId = insertDto.ProblemId,
            UserId = insertDto.UserId,
            Stars = 3,
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            await commandDbContext.UserSolutions.AddAsync(solution, cancellationToken);
            await commandDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.InternalError(e.Message));
        }

        var objectPath = $"users/{solution.UserId}/problems/autosave/{solution.ProblemId}.xml";

        await awsS3Client.DeleteDocumentAsync(objectPath, cancellationToken: cancellationToken);
        
        return await PostUserSolutionCodeToS3Async(new UserSolutionPartialS3
        {
            CodeB64 = insertDto.CodeB64,
            UserId = insertDto.UserId,
            UserSolutionId = solution.SolutionId
        });
    }

    public async Task<Result<RewardsDto, ErrorObject<string>>> AddCoinsAndExperienceAsync(SolutionRewardDto rewardDto, CancellationToken cancellationToken = default)
    {
        if (await commandDbContext.UserSolutions.CountAsync(
                s => s.ProblemId == rewardDto.ProblemId && s.UserId == rewardDto.UserId,
                cancellationToken: cancellationToken) != 0)
            return Result<RewardsDto, ErrorObject<string>>.Ok(new RewardsDto
            {
                AwardedCoins = 0,
                AwardedExperience = 0
            });

        var difficultyRes = await commandDbContext.Problems
            .Include(p => p.Difficulty)
            .Where(p => p.ProblemId == rewardDto.ProblemId)
            .Select(p => p.Difficulty)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (difficultyRes == null)
            return Result<RewardsDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"difficulty for problem: {rewardDto.ProblemId} not found"));

        var user = await commandDbContext.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == rewardDto.UserId, cancellationToken);

        if (user == null)
            return Result<RewardsDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"user {rewardDto.UserId} not found"));

        var coinsAwarded = (int)(awardsConfig.Value.BaselineCoins * difficultyRes.RewardScaler);
        var experienceAwarded = (int)(awardsConfig.Value.BaselineExperience * difficultyRes.RewardScaler);
        
        user.Coins += coinsAwarded;
        user.Experience += experienceAwarded;

        await commandDbContext.SaveChangesAsync(cancellationToken);

        return Result<RewardsDto, ErrorObject<string>>.Ok(new RewardsDto
        {
            AwardedCoins = coinsAwarded,
            AwardedExperience = experienceAwarded
        });
    }

    private async Task<Result<bool, ErrorObject<string>>> PostUserSolutionCodeToS3Async(UserSolutionPartialS3 insertDto)
    {
        if (insertDto.UserId == Guid.Empty || insertDto.UserId == Guid.Empty)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest(""));
        }

        try
        {
            await awsS3Client.PostXmlObjectAsync($"users/{insertDto.UserSolutionId}/solutions/${insertDto.UserSolutionId}.xml", insertDto);
            return Result<bool, ErrorObject<string>>.Ok(true);   
        }
        catch (Exception e)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest(e.Message));
        }
    }
    
    private async Task<Result<bool, ErrorObject<string>>> DropLastCheckpointAsync(AutoSaveDropDto dropDto, CancellationToken cancellationToken = default)
    {
        var objectPath = $"users/{dropDto.UserId}/problems/autosave/{dropDto.ProblemId}.xml";
        return await awsS3Client.DeleteDocumentAsync(objectPath, cancellationToken: cancellationToken);
    }
}
public class SubmissionInsertDto
{
    public required long CodeRuntimeSubmitted { get; set; }
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
    public required string CodeB64 { get; set; }
}

public class UserSolutionPartialS3
{
    public required string CodeB64 { get; set; }
    internal Guid UserSolutionId { get; set; }
    internal Guid UserId { get; set; }
}

internal class AutoSaveDropDto
{
    internal required Guid UserId { get; set; }
    internal required Guid ProblemId { get; set; }
}

public class SolutionRewardDto
{
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
}

public class RewardsDto
{
    public int AwardedCoins { get; set; }
    public int AwardedExperience { get; set; }
}