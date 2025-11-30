using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.PurchaseItem;
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

        builder.Services.AddScoped<IPurchaseItemService, PurchaseItemService>();
        builder.Services.AddScoped<IPurchaseItemRepository, PurchaseItemRepository>();

        builder.Services.AddScoped<ICreateItemRepository, CreateItemRepository>();
        builder.Services.AddScoped<ICreateItemService, CreateItemService>();
    }
}