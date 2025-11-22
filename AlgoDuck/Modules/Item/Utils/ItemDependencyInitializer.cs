using AlgoDuck.Modules.Item.Repositories;
using AlgoDuck.Modules.Item.Services;

namespace AlgoDuck.Modules.Item.Utils;

internal static class ItemDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IItemRepository, ItemRepository>();
        builder.Services.AddScoped<IItemService, ItemService>();    }
}