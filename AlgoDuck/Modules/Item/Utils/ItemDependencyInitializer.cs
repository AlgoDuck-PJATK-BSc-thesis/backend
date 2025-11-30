using AlgoDuck.Modules.Item.Queries.GetAllItemsPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;

namespace AlgoDuck.Modules.Item.Utils;

internal static class ItemDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOwnedItemsRepository, OwnedItemsRepository>();
        builder.Services.AddScoped<IOwnedItemsService, OwnedItemsService>();

        builder.Services.AddScoped<IAllItemsRepository, AllItemsRepository>();
        builder.Services.AddScoped<IAllItemService, AllItemService>();
    }
    
}