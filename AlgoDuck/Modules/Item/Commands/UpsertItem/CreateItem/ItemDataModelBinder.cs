using System.Text.Json;
using AlgoDuck.Modules.Item.Commands.CreateItem.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AlgoDuck.Modules.Item.Commands.CreateItem;

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
