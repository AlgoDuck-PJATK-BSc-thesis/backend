using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Commands.PurchaseItem;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Modules.Item.Purchase;

public class PurchaseItemRepositoryTests : IDisposable
{
    private readonly ApplicationCommandDbContext _context;
    private readonly PurchaseItemRepository _repository;

    public PurchaseItemRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationCommandDbContext(options);
        _repository = new PurchaseItemRepository(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Coins = 100,
            Purchases = new List<Models.Purchase>()
        };

        var item = new Models.Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Test Item",
            Price = 50,
            Purchasable = true
        };

        _context.ApplicationUsers.Add(user);
        _context.Items.Add(item);
        _context.SaveChanges();
    }

    [Fact]
    public async Task PurchaseItemAsync_WithSufficientFunds_CompletesSuccessfully()
    {
        var user = await _context.ApplicationUsers.FirstAsync();
        var item = await _context.Items.FirstAsync();
        
        var request = new PurchaseRequestInternalDto
        {
            RequestingUserId = user.Id,
            PurchaseRequestDto = new PurchaseRequestDto { ItemId = item.ItemId }
        };

        var result = await _repository.PurchaseItemAsync(request, CancellationToken.None);

        Assert.Equal(item.ItemId, result.ItemId);
        
        var updatedUser = await _context.ApplicationUsers
            .Include(u => u.Purchases)
            .FirstAsync(u => u.Id == user.Id);
        
        Assert.Equal(50, updatedUser.Coins);
        Assert.Single(updatedUser.Purchases);
    }

    [Fact]
    public async Task PurchaseItemAsync_WithInsufficientFunds_ThrowsException()
    {
        var user = await _context.ApplicationUsers.FirstAsync();
        user.Coins = 10; 
        await _context.SaveChangesAsync();
        
        var item = await _context.Items.FirstAsync();
        var request = new PurchaseRequestInternalDto
        {
            RequestingUserId = user.Id,
            PurchaseRequestDto = new PurchaseRequestDto { ItemId = item.ItemId }
        };

        await Assert.ThrowsAsync<NotEnoughCurrencyException>(
            () => _repository.PurchaseItemAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task PurchaseItemAsync_WhenAlreadyOwned_ThrowsException()
    {
        var user = await _context.ApplicationUsers.Include(u => u.Purchases).FirstAsync();
        var item = await _context.Items.FirstAsync();
        
        user.Purchases.Add(new Models.Purchase { ItemId = item.ItemId, UserId = user.Id });
        await _context.SaveChangesAsync();
        
        var request = new PurchaseRequestInternalDto
        {
            RequestingUserId = user.Id,
            PurchaseRequestDto = new PurchaseRequestDto { ItemId = item.ItemId }
        };

        await Assert.ThrowsAsync<ItemAlreadyPurchasedException>(
            () => _repository.PurchaseItemAsync(request, CancellationToken.None)
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}