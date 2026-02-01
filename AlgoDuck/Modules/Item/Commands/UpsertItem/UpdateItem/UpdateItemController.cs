using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.CreateItem.Types;
using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;
using AlgoDuck.Shared.Attributes;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.UpdateItem;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/item")]
public class UpdateItemController : ControllerBase
{
    private readonly IUpdateItemService _updateItemService;
    private readonly IValidator<ItemUpdateRequestDto> _validator;
    public UpdateItemController(IUpdateItemService updateItemService, IValidator<ItemUpdateRequestDto> validator)
    {
        _updateItemService = updateItemService;
        _validator = validator;
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateItemAsync([FromForm] ItemUpdateRequestDto updateItemUpdateRequestDto, CancellationToken cancellation)
    {
        
        var validationResult = await _validator.ValidateAsync(updateItemUpdateRequestDto, cancellation);
        if (!validationResult.IsValid)
            return new ObjectResult(new StandardApiResponse<IEnumerable<ValidationFailure>>
            {
                Status = Status.Error,
                Body = validationResult.Errors
            }){ StatusCode = 422 };

        return await _updateItemService.UpdateItemAsync(updateItemUpdateRequestDto, cancellation).ToActionResultAsync();
    }   
}


public interface IUpdateItemService
{
    public Task<Result<ItemUpdateResult, ErrorObject<string>>> UpdateItemAsync(ItemUpdateRequestDto updateItemUpdateRequestDto, CancellationToken cancellationToken = default);
}

public class UpdateItemService : IUpdateItemService
{
    private readonly IUpdateItemRepository _updateItemRepository;

    public UpdateItemService(IUpdateItemRepository updateItemRepository)
    {
        _updateItemRepository = updateItemRepository;
    }

    public async Task<Result<ItemUpdateResult, ErrorObject<string>>> UpdateItemAsync(ItemUpdateRequestDto updateItemUpdateRequestDto, CancellationToken cancellationToken = default)
    {
        return await _updateItemRepository.UpdateItemAsync(updateItemUpdateRequestDto, cancellationToken);
    }
}

public interface IUpdateItemRepository
{
    public Task<Result<ItemUpdateResult, ErrorObject<string>>> UpdateItemAsync(ItemUpdateRequestDto updateItemUpdateRequestDto, CancellationToken cancellationToken = default);
}

public class UpdateItemRepository : IUpdateItemRepository
{
    private readonly IAwsS3Client _s3Client;
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IOptions<SpriteLegalFileNamesConfiguration> _legalFileNames;

    public UpdateItemRepository(ApplicationCommandDbContext dbContext, IAwsS3Client s3Client, IOptions<SpriteLegalFileNamesConfiguration> legalFileNames)
    {
        _dbContext = dbContext;
        _s3Client = s3Client;
        _legalFileNames = legalFileNames;
    }

    public async Task<Result<ItemUpdateResult, ErrorObject<string>>> UpdateItemAsync(ItemUpdateRequestDto updateItemUpdateRequestDto, CancellationToken cancellationToken = default)
    {
        return updateItemUpdateRequestDto.ItemData switch
        {
            DuckData duckData => await UpdateDuckItemAsync(updateItemUpdateRequestDto, duckData, cancellationToken)
                .BindAsync(async duck => await PostSprites(duck, updateItemUpdateRequestDto.Sprites, cancellationToken)),
            PlantData plantData => await UpdatePlantItemAsync(updateItemUpdateRequestDto, plantData, cancellationToken)
                .BindAsync(async plant => await PostSprites(plant, updateItemUpdateRequestDto.Sprites, cancellationToken)),
            _ => Result<ItemUpdateResult, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("Could not create item"))
        };
    }

    private async Task<Result<DuckItem, ErrorObject<string>>> UpdateDuckItemAsync(ItemUpdateRequestDto updateRequestDto,
        DuckData duckData, CancellationToken cancellationToken = default)
    {
        var duckItem = await _dbContext.DuckItems.FirstOrDefaultAsync(i => i.ItemId == updateRequestDto.ItemId, cancellationToken: cancellationToken);
        
        if (duckItem == null)
            return Result<DuckItem, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"No duck type item found for id:  {updateRequestDto.ItemId}"));
        
        if (!_dbContext.Rarities.Any(r => r.RarityId == updateRequestDto.RarityId))
            return Result<DuckItem, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Invalid item rarity"));
        
        duckItem.Description = updateRequestDto.Description;
        duckItem.Name = updateRequestDto.ItemName;
        duckItem.Price = updateRequestDto.ItemCost;
        duckItem.RarityId = updateRequestDto.RarityId;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result<DuckItem, ErrorObject<string>>.Ok(duckItem);
    }


    private async Task<Result<PlantItem, ErrorObject<string>>> UpdatePlantItemAsync(ItemUpdateRequestDto updateRequestDto,
        PlantData duckData, CancellationToken cancellationToken = default)
    {
        var plantItem = await _dbContext.PlantItems.FirstOrDefaultAsync(i => i.ItemId == updateRequestDto.ItemId, cancellationToken: cancellationToken);
        
        if (plantItem == null)
            return Result<PlantItem, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"No plant type item found for id:  {updateRequestDto.ItemId}"));
        
        if (!_dbContext.Rarities.Any(r => r.RarityId == updateRequestDto.RarityId))
            return Result<PlantItem, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Invalid item rarity"));
        
        plantItem.Description = updateRequestDto.Description;
        plantItem.Name = updateRequestDto.ItemName;
        plantItem.Price = updateRequestDto.ItemCost;
        plantItem.RarityId = updateRequestDto.RarityId;
        plantItem.Width = duckData.Width;
        plantItem.Height = duckData.Height;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result<PlantItem, ErrorObject<string>>.Ok(plantItem);
    }
    
    private async Task<Result<ItemUpdateResult, ErrorObject<string>>> PostSprites(Models.Item item, IFormFileCollection sprites, CancellationToken cancellationToken = default)
    {
        
        var itemTypeName = item switch
        {
            DuckItem => "duck",
            PlantItem => "plant",
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
        var objectKeyBase = $"{itemTypeName}/{item.ItemId}";

        List<FilePostingResult> results = [];
        foreach (var sprite in sprites)
        {
            if (!_legalFileNames.Value.LegalFileNames.TryGetValue(itemTypeName, out var legalName)) continue;
            var spritePath = $"{objectKeyBase}/{sprite.FileName}";

            if (!legalName.Contains(Path.GetFileName(sprite.FileName)))
                return Result<ItemUpdateResult, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Illegal file name"));

            var result = await _s3Client.PostRawFileAsync(
                path: spritePath,
                fileContents: sprite.OpenReadStream(),
                contentType: sprite.ContentType,
                bucketType: S3BucketType.Content,
                cancellationToken: cancellationToken);
            
            results.Add(result.Match<FilePostingResult>(
                ok => new FilePostingSuccessResult
                {
                    FileName = sprite.FileName,
                },
                err => new FilePostingFailureResult
                {
                    FileName = sprite.FileName,
                    Reason = err.Body,
                }
            ));
        }
        return Result<ItemUpdateResult, ErrorObject<string>>.Ok(new ItemUpdateResult
        {
            ItemId = item.ItemId,
            Files = results
        });
    }

}

public class ItemUpdateResult
{
    public required Guid ItemId { get; set; }
    public required ICollection<FilePostingResult> Files { get; set; }
}


public class ItemUpdateRequestDto
{
    public required Guid ItemId { get; set; }
    [MaxLength(128)]
    public required string ItemName { get; set; }
    [MaxLength(128)]
    public string? Description { get; set; } 
    [Range(minimum: 0, maximum: int.MaxValue)]
    public required int ItemCost { get; set; }
    public required Guid RarityId { get; set; }
    [ModelBinder(BinderType = typeof(ItemDataModelBinder))]
    public required IItemTypeSpecificData ItemData { get; set; }
    [FromForm] 
    [MaxFileCount(10)] 
    [MaxFileSize(512 * 1024)]
    public required IFormFileCollection Sprites { get; set; }
    internal Guid CreatedByUserId { get; set; }
}
