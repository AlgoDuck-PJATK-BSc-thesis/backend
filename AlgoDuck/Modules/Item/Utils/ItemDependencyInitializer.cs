using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.DeleteItem;
using AlgoDuck.Modules.Item.Commands.DropItemAsActive;
using AlgoDuck.Modules.Item.Commands.EmplacePlantOnHomePage;
using AlgoDuck.Modules.Item.Commands.PurchaseItem;
using AlgoDuck.Modules.Item.Commands.RemovePlantFromHomepage;
using AlgoDuck.Modules.Item.Commands.SelectItemAsActive;
using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;
using AlgoDuck.Modules.Item.Commands.UpsertItem.LoadFormState;
using AlgoDuck.Modules.Item.Commands.UpsertItem.UpdateItem;
using AlgoDuck.Modules.Item.Queries.AdminGetAllItemsPagedFilterable;
using AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetAllItemRarities;
using AlgoDuck.Modules.Item.Queries.GetAllOwnedDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetAllOwnedPlantsPaged;
using AlgoDuck.Modules.Item.Queries.GetAllPlantsPaged;
using AlgoDuck.Modules.Item.Queries.GetMyIconItem;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetUserPreviewData;

namespace AlgoDuck.Modules.Item.Utils;

internal static class ItemDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOwnedItemsRepository, OwnedUsedItemsRepository>();
        builder.Services.AddScoped<IOwnedItemsService, OwnedUsedItemsService>();

        builder.Services.AddScoped<IAllDucksRepository, AllDucksRepository>();
        builder.Services.AddScoped<IAllDucksService, AllDucksService>();
        
        builder.Services.AddScoped<IAllPlantsRepository, AllPlantsRepository>();
        builder.Services.AddScoped<IAllPlantsService, AllPlantsService>();

        builder.Services.AddScoped<IPurchaseItemService, PurchaseItemService>();
        builder.Services.AddScoped<IPurchaseItemRepository, PurchaseItemRepository>();

        builder.Services.AddScoped<ICreateItemRepository, CreateItemRepository>();
        builder.Services.AddScoped<ICreateItemService, CreateItemService>();
        
        builder.Services.AddScoped<ISelectItemRepository, SelectItemRepository>();
        builder.Services.AddScoped<ISelectItemService, SelectItemService>();
        
        builder.Services.AddScoped<IDropItemRepository, DropItemRepository>();
        builder.Services.AddScoped<IDropItemService, DropItemService>();

        builder.Services.AddScoped<IOwnedDucksRepository, OwnedDucksRepository>();
        builder.Services.AddScoped<IOwnedDucksService, OwnedDucksService>();

        builder.Services.AddScoped<IOwnedPlantsRepository, OwnedPlantsRepository>();
        builder.Services.AddScoped<IOwnedPlantsService, OwnedPlantsService>();
        
        builder.Services.AddScoped<IEmplacePlantService, EmplacePlantService>();
        builder.Services.AddScoped<IEmplacePlantRepository, EmplacePlantRepository>();
        
        builder.Services.AddScoped<IAllItemsPagedRepository, AllItemsPagedRepository>();
        builder.Services.AddScoped<IAllItemsPagedService, AllItemsPagedService>();

        builder.Services.AddScoped<IFullItemDetailsService, FullItemDetailsService>();
        builder.Services.AddScoped<IFullItemDetailsRepository, FullItemDetailsRepository>();

        builder.Services.AddScoped<IDeleteItemService, DeleteItemService>();
        builder.Services.AddScoped<IDeleteItemRepository, DeleteItemRepository>();
        
        builder.Services.AddScoped<IAllItemRaritiesRepository, AllItemRaritiesRepository>();
        builder.Services.AddScoped<IAllItemRaritiesService, AllItemRaritiesService>();

        builder.Services.AddScoped<IRemovePlantService, RemovePlantService>();
        builder.Services.AddScoped<IRemovePlantRepository, RemovePlantRepository>();
        
        builder.Services.AddScoped<ILoadFormStateRepository, LoadFormStateRepository>();
        builder.Services.AddScoped<ILoadFormStateService, LoadFormStateService>();
        
        builder.Services.AddScoped<IUpdateItemRepository, UpdateItemRepository>();
        builder.Services.AddScoped<IUpdateItemService, UpdateItemService>();

        builder.Services.AddScoped<IGetMySelectedIconService, GetMySelectedIconService>();
        builder.Services.AddScoped<IGetMySelectedIconRepository, GetMySelectedIconRepository>();

        builder.Services.AddScoped<IGetUserPreviewService, GetUserPreviewService>();
        builder.Services.AddScoped<IGetUserPreviewRepository, GetUserPreviewRepository>();

        builder.Services.Configure<AwardsConfig>(
            builder.Configuration.GetSection("Awards"));
        
        builder.Services.Configure<SpriteLegalFileNamesConfiguration>(
            builder.Configuration.GetSection("SpriteLegalFileNames"));
        
    }
}