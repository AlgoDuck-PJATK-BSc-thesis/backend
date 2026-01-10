using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Commands.CreateItem.Types;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.LoadFormState;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/item/admin/form")]
public class LoadFormStateController : ControllerBase
{
    private readonly ILoadFormStateService _loadFormStateService;

    public LoadFormStateController(ILoadFormStateService loadFormStateService)
    {
        _loadFormStateService = loadFormStateService;
    }

    [HttpGet]
    public async Task<IActionResult> LoadFormStateASync([FromQuery] Guid itemId, CancellationToken cancellationToken)
    {
        return await _loadFormStateService.LoadItemFormStateAsync(itemId, cancellationToken).ToActionResultAsync();
    }
}

public interface ILoadFormStateService
{
    public Task<Result<LoadItemFormDto, ErrorObject<string>>> LoadItemFormStateAsync(Guid itemId, CancellationToken cancellationToken = default);
}

public class LoadFormStateService : ILoadFormStateService
{
    private readonly ILoadFormStateRepository _loadFormStateRepository;

    public LoadFormStateService(ILoadFormStateRepository loadFormStateRepository)
    {
        _loadFormStateRepository = loadFormStateRepository;
    }

    public async Task<Result<LoadItemFormDto, ErrorObject<string>>> LoadItemFormStateAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _loadFormStateRepository.LoadItemFormStateAsync(itemId, cancellationToken);
    }
}

public interface ILoadFormStateRepository
{
    public Task<Result<LoadItemFormDto, ErrorObject<string>>> LoadItemFormStateAsync(Guid itemId, CancellationToken cancellationToken = default);
}

public class LoadFormStateRepository : ILoadFormStateRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public LoadFormStateRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<LoadItemFormDto, ErrorObject<string>>> LoadItemFormStateAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Items.Where(item => item.ItemId == itemId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (item == null)
            return Result<LoadItemFormDto, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"item for id: {itemId} not found"));
        
        return Result<LoadItemFormDto, ErrorObject<string>>.Ok(new LoadItemFormDto
        {
            ItemId = itemId,
            ItemName = item.Name,
            Description = item.Description,
            ItemCost = item.Price,
            ItemData = item switch
            {
                DuckItem => new DuckData(), // again. Technically empty but left in for clarity 
                PlantItem p => new PlantData
                {
                    Height = p.Height,
                    Width = p.Height,
                },
                _ => throw new ArgumentOutOfRangeException()
            },
            RarityId = item.RarityId,
        });
    }
}




public class LoadItemFormDto
{
    public required Guid ItemId { get; set; }
    public required string ItemName { get; set; }
    public string? Description { get; set; } 
    public required int ItemCost { get; set; }
    public required Guid RarityId { get; set; }
    public required IItemTypeSpecificData ItemData { get; set; }
}
