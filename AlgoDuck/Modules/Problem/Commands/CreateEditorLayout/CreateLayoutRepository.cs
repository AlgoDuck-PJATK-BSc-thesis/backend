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
        var userConfig = await dbContext.UserConfigs.Where(e => e.UserId == createDto.UserId).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (userConfig == null)
        {
            userConfig = new UserConfig
            {
                EmailNotificationsEnabled = false,
                IsDarkMode = true,
                PushNotificationsEnabled = false,
                UserId = createDto.UserId,
                IsHighContrast = false,
                Language = "en"
            };
            dbContext.UserConfigs.Add(userConfig);
        }
        
        var newEditorLayout = new EditorLayout
        {
            EditorThemeId = Guid.Parse("276cc32e-a0bd-408e-b6f0-0f4e3ff80796") /*TODO: not cool. EDIT: Not cool either*/,
            LayoutName = createDto.LayoutName
        };
        
        userConfig.EditorLayouts.Add(newEditorLayout);
        
        
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var res = JsonSerializer.Deserialize<object>(createDto.LayoutContent);
            if (res == null)
                return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("Could not create layout"));
            
            var postResult = await awsS3Client.PostJsonObjectAsync($"users/{createDto.UserId}/layouts/{newEditorLayout.EditorLayoutId}.json", res, cancellationToken: cancellationToken);
            
            if (postResult.IsErr)
                return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(postResult.AsT1);

            return Result<LayoutCreateResultDto, ErrorObject<string>>.Ok(new LayoutCreateResultDto
            {
                LayoutId = newEditorLayout.EditorLayoutId,
            });
        }
        catch (JsonException)
        {
            return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Invalid layout format"));
        }
    }

    public async Task<Result<ICollection<EditorLayoutDto>, ErrorObject<string>>> GetOwnedLayoutCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Result<ICollection<EditorLayoutDto>, ErrorObject<string>>.Ok(
            await dbContext.EditorLayouts.AsNoTracking().Include(e => e.UserConfig).Where(e => e.UserConfig.UserId == userId)
                .Select(e => new EditorLayoutDto
                {
                    Id = e.EditorLayoutId,
                    Name = e.LayoutName
                }).ToListAsync(cancellationToken: cancellationToken));   
    }
}

public class EditorLayoutDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}