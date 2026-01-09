using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;


public interface ICreateLayoutRepository
{
    public Task<Result<LayoutCreateResultDto, ErrorObject<string>>> CreateLayoutAsync(LayoutCreateDto createDto,
        CancellationToken cancellationToken = default);
    
    public Task<Result<ICollection<EditorLayoutDto>, ErrorObject<string>>> GetOwnedLayoutCountAsync(Guid userId, CancellationToken cancellationToken = default);

}

public class CreateLayoutRepository(
    ApplicationCommandDbContext dbContext,
    IAwsS3Client awsS3Client
    ) : ICreateLayoutRepository
{
    public async Task<Result<LayoutCreateResultDto, ErrorObject<string>>> CreateLayoutAsync(LayoutCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.ApplicationUsers.Where(e => e.Id == createDto.UserId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (user == null)
            return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"User with id: {createDto.UserId} not found"));

        var newLayout = new EditorLayout
        {
            LayoutName = createDto.LayoutName
        };
        
        user.EditorLayouts.Add(new OwnsLayout
        {
            UserId = createDto.UserId,
            LayoutId = newLayout.EditorLayoutId,
            Layout = newLayout,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        
        try
        {
            var res = JsonSerializer.Deserialize<object>(createDto.LayoutContent);
            if (res == null)
                return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("Could not create layout"));
            
            var postResult = await awsS3Client.PostJsonObjectAsync($"users/{createDto.UserId}/layouts/{newLayout.EditorLayoutId}.json", res, cancellationToken: cancellationToken);
            
            if (postResult.IsErr)
                return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(postResult.AsT1);

            return Result<LayoutCreateResultDto, ErrorObject<string>>.Ok(new LayoutCreateResultDto
            {
                LayoutId = newLayout.EditorLayoutId
            });
        }
        catch (JsonException)
        {
            return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Invalid layout format"));
        }
    }

    public async Task<Result<ICollection<EditorLayoutDto>, ErrorObject<string>>> GetOwnedLayoutCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.ApplicationUsers
            .Include(e => e.EditorLayouts)
            .ThenInclude(e => e.Layout)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == userId, cancellationToken: cancellationToken);
        if (user == null)
            return Result<ICollection<EditorLayoutDto>, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"User with id: {userId} not found"));
        
        return Result<ICollection<EditorLayoutDto>, ErrorObject<string>>.Ok(user.EditorLayouts.Select(l => new EditorLayoutDto()
        {
            Id = l.LayoutId,
            Name = l.Layout.LayoutName
        }).ToList());
    }
}

public class EditorLayoutDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}

public class EditorLayoutS3Partial
{
    public required Guid Id { get; set; }
    
}