using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.CreateItem.Types;
using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem.Types;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;

public interface ICreateItemRepository
{
    public Task<Result<ItemCreateResponseDto, ErrorObject<string>>> CreateItemAsync(CreateItemRequestDto createItemRequestDto,
        CancellationToken cancellationToken = default);
}

public class CreateItemRepository : ICreateItemRepository
{
    private readonly IAwsS3Client _s3Client;
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IOptions<SpriteLegalFileNamesConfiguration> _legalFileNames;


    public CreateItemRepository(IAwsS3Client s3Client, ApplicationCommandDbContext dbContext, IOptions<SpriteLegalFileNamesConfiguration> legalFileNames)
    {
        _s3Client = s3Client;
        _dbContext = dbContext;
        _legalFileNames = legalFileNames;
    }

    public async Task<Result<ItemCreateResponseDto, ErrorObject<string>>> CreateItemAsync(CreateItemRequestDto createItemRequestDto,
        CancellationToken cancellationToken = default)
    {

        return createItemRequestDto.ItemData switch
        {
            DuckData duckData => await CreateDuckItemAsync(createItemRequestDto, duckData, cancellationToken)
                .BindAsync(async duck => await PostSprites(duck, createItemRequestDto.Sprites, cancellationToken)),
            PlantData plantData => await CreatePlantItemAsync(createItemRequestDto, plantData, cancellationToken)
                .BindAsync(async plant => await PostSprites(plant, createItemRequestDto.Sprites, cancellationToken)),
            _ => Result<ItemCreateResponseDto, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("Could not create item"))
        };
    }


    private async Task<Result<DuckItem, ErrorObject<string>>> CreateDuckItemAsync(CreateItemRequestDto requestDto,
        DuckData duckData, CancellationToken cancellationToken = default) /* duck data kept intentionally in spite of being unused. For the sake of consistency */
    {
        var newDuck = new DuckItem
        {
            Name = requestDto.ItemName,
            Description = requestDto.Description,
            RarityId = requestDto.RarityId,
            Price = requestDto.ItemCost,
            Purchasable = true,
        };
        _dbContext.DuckItems.Add(newDuck);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<DuckItem, ErrorObject<string>>.Ok(newDuck);
    }
    
    private async Task<Result<PlantItem, ErrorObject<string>>> CreatePlantItemAsync(CreateItemRequestDto requestDto,
        PlantData duckData, CancellationToken cancellationToken = default)
    {
        var newPlant = new PlantItem
        {
            Name = requestDto.ItemName,
            Description = requestDto.Description,
            RarityId = requestDto.RarityId,
            Price = requestDto.ItemCost,
            Purchasable = true,
            Width = duckData.Width,
            Height = duckData.Height
        };
        _dbContext.PlantItems.Add(newPlant);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<PlantItem, ErrorObject<string>>.Ok(newPlant);
    }

    private async Task<Result<ItemCreateResponseDto, ErrorObject<string>>> PostSprites(Models.Item item, IFormFileCollection sprites, CancellationToken cancellationToken = default)
    {
        var itemTypeName = item switch
        {
            DuckItem => "duck",
            PlantItem => "plant",
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
        
        var objectKeyBase = $"{itemTypeName}s/{item.ItemId}";

        List<FilePostingResult> results = [];
        foreach (var sprite in sprites)
        {
            if (!_legalFileNames.Value.LegalFileNames.TryGetValue(itemTypeName, out var legalName)) continue;
            var spritePath = $"{objectKeyBase}/{sprite.FileName}";

            if (!legalName.Contains(Path.GetFileNameWithoutExtension(sprite.FileName).ToLowerInvariant()))
                return Result<ItemCreateResponseDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Illegal file name"));
            
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
        return Result<ItemCreateResponseDto, ErrorObject<string>>.Ok(new ItemCreateResponseDto
        {
            CreatedItemGuid = item.ItemId,
            Files = results
        });
    }
}