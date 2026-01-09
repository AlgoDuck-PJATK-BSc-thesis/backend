using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Attributes;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AlgoDuck.Modules.Item.Commands.CreateItem;

[ApiController]
[Route("/api/admin/item")]
[Authorize(Roles = "admin")]
public class CreateItemController(
    ICreateItemService createItemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateItemAsync(
        [FromForm] CreateItemRequestDto createItemDto, 
        CancellationToken cancellation)
    {
        Console.WriteLine("got request");
        return await User
            .GetUserId()
            .BindAsync(async user =>
            {
                createItemDto.CreatedByUserId = user;
                return await createItemService.CreateItemAsync(createItemDto, cancellation);
            }).ToActionResultAsync();
    }    
}



public class CreateItemRequestDto
{

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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DuckData), "duck")]
[JsonDerivedType(typeof(PlantData), "plant")]
public interface IItemTypeSpecificData;

public class DuckData : IItemTypeSpecificData;

public class PlantData : IItemTypeSpecificData
{
    [Range(minimum: 0, maximum: byte.MaxValue)]
    public required byte Width { get; set; }
    [Range(minimum: 0, maximum: byte.MaxValue)]
    public required byte Height { get; set; }
}

public class ItemCreateResponseDto
{
    public required Guid CreatedItemGuid { get; set; }
    public required ICollection<FilePostingResult> Files { get; set; }
}

public abstract class FilePostingResult
{
    public required string FileName { get; set; }
    public Status Result { get; protected set; }   
}

public class FilePostingSuccessResult : FilePostingResult
{
    public FilePostingSuccessResult()
    {
        Result = Status.Success;
    }
}

public class FilePostingFailureResult : FilePostingResult
{
    public required string Reason { get; set; }
    public FilePostingFailureResult()
    {
        Result = Status.Error;
    }
}


public class ItemDataModelBinder : IModelBinder
{
    private readonly JsonSerializerOptions _defaultJsonSerializerOptions = new()
    { 
        PropertyNameCaseInsensitive = true 
    };
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue("itemData");
        
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var jsonString = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(jsonString))
            return Task.CompletedTask;
        
        try
        {
            bindingContext.Result = ModelBindingResult.Success(JsonSerializer.Deserialize<IItemTypeSpecificData>(jsonString, _defaultJsonSerializerOptions));
        }
        catch (JsonException)
        {
            bindingContext.ModelState.AddModelError( bindingContext.ModelName, "Invalid JSON for ItemData");
        }

        return Task.CompletedTask;
    }
}
