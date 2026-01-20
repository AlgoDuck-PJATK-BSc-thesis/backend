using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Shared.Utilities;

public sealed class DefaultDuckServiceTests
{
    static ApplicationCommandDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task EnsureAlgoduckOwnedAndSelectedAsync_WhenUserNotFound_DoesNothing()
    {
        using var db = CreateDbContext();
        var userManager = CreateUserManagerMock();

        userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var svc = new DefaultDuckService(db, userManager.Object);

        await svc.EnsureAlgoduckOwnedAndSelectedAsync(Guid.NewGuid(), CancellationToken.None);

        var purchases = await db.Set<ItemOwnership>().CountAsync();
        Assert.Equal(0, purchases);
    }

    [Fact]
    public async Task EnsureAlgoduckOwnedAndSelectedAsync_WhenUserIsAdmin_DoesNothing()
    {
        using var db = CreateDbContext();

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "admin", Email = "a@a.com", EmailConfirmed = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.IsInRoleAsync(user, "admin")).ReturnsAsync(true);

        var svc = new DefaultDuckService(db, userManager.Object);

        await svc.EnsureAlgoduckOwnedAndSelectedAsync(user.Id, CancellationToken.None);

        var purchases = await db.Set<ItemOwnership>().CountAsync();
        Assert.Equal(0, purchases);
    }

    [Fact]
    public async Task EnsureAlgoduckOwnedAndSelectedAsync_WhenAlgoduckMissing_Throws()
    {
        using var db = CreateDbContext();

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u", Email = "u@u.com", EmailConfirmed = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.IsInRoleAsync(user, "admin")).ReturnsAsync(false);

        var svc = new DefaultDuckService(db, userManager.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.EnsureAlgoduckOwnedAndSelectedAsync(user.Id, CancellationToken.None));
    }

    [Fact]
    public async Task EnsureAlgoduckOwnedAndSelectedAsync_WhenUserHasOtherSelectedDuck_DoesNotOverrideSelection_ButEnsuresAlgoduckOwnedAndInPond()
    {
        using var db = CreateDbContext();

        var rarityId = Guid.NewGuid();
        db.Rarities.Add(new Rarity { RarityId = rarityId, RarityName = "COMMON" });

        var algoduckItemId = Guid.NewGuid();
        var otherDuckItemId = Guid.NewGuid();

        db.DuckItems.Add(new DuckItem
        {
            ItemId = algoduckItemId,
            Name = "algoduck",
            Description = "",
            Price = 0,
            Purchasable = true,
            RarityId = rarityId
        });

        db.DuckItems.Add(new DuckItem
        {
            ItemId = otherDuckItemId,
            Name = "pirate",
            Description = "",
            Price = 10,
            Purchasable = true,
            RarityId = rarityId
        });

        var userId = Guid.NewGuid();

        db.DuckOwnerships.Add(new DuckOwnership
        {
            UserId = userId,
            ItemId = otherDuckItemId,
            SelectedAsAvatar = true,
            SelectedForPond = false
        });

        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "u@u.com", EmailConfirmed = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.IsInRoleAsync(user, "admin")).ReturnsAsync(false);

        var svc = new DefaultDuckService(db, userManager.Object);

        await svc.EnsureAlgoduckOwnedAndSelectedAsync(userId, CancellationToken.None);

        var algoduckOwnership = await db.DuckOwnerships.SingleAsync(o => o.UserId == userId && o.ItemId == algoduckItemId);
        Assert.False(algoduckOwnership.SelectedAsAvatar);
        Assert.True(algoduckOwnership.SelectedForPond);

        var other = await db.DuckOwnerships.SingleAsync(o => o.UserId == userId && o.ItemId == otherDuckItemId);
        Assert.True(other.SelectedAsAvatar);

        var selectedCount = await db.DuckOwnerships.CountAsync(o => o.UserId == userId && o.SelectedAsAvatar);
        Assert.Equal(1, selectedCount);
    }

    [Fact]
    public async Task EnsureAlgoduckOwnedAndSelectedAsync_WhenNoSelectedAvatar_CreatesAlgoduckAndSelectsIt_ForAvatarAndPond()
    {
        using var db = CreateDbContext();

        var rarityId = Guid.NewGuid();
        db.Rarities.Add(new Rarity { RarityId = rarityId, RarityName = "COMMON" });

        var algoduckItemId = Guid.NewGuid();

        db.DuckItems.Add(new DuckItem
        {
            ItemId = algoduckItemId,
            Name = "algoduck",
            Description = "",
            Price = 0,
            Purchasable = true,
            RarityId = rarityId
        });

        var userId = Guid.NewGuid();

        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "u@u.com", EmailConfirmed = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.IsInRoleAsync(user, "admin")).ReturnsAsync(false);

        var svc = new DefaultDuckService(db, userManager.Object);

        await svc.EnsureAlgoduckOwnedAndSelectedAsync(userId, CancellationToken.None);

        var algoduckOwnership = await db.DuckOwnerships.SingleAsync(o => o.UserId == userId && o.ItemId == algoduckItemId);
        Assert.True(algoduckOwnership.SelectedAsAvatar);
        Assert.True(algoduckOwnership.SelectedForPond);

        var selectedCount = await db.DuckOwnerships.CountAsync(o => o.UserId == userId && o.SelectedAsAvatar);
        Assert.Equal(1, selectedCount);
    }

    [Fact]
    public async Task EnsureAlgoduckOwnedAndSelectedAsync_WhenUserAlreadyOwnsAlgoduck_AndNoOtherSelected_SelectsAlgoduck_AndSetsPondTrue()
    {
        using var db = CreateDbContext();

        var rarityId = Guid.NewGuid();
        db.Rarities.Add(new Rarity { RarityId = rarityId, RarityName = "COMMON" });

        var algoduckItemId = Guid.NewGuid();

        db.DuckItems.Add(new DuckItem
        {
            ItemId = algoduckItemId,
            Name = "algoduck",
            Description = "",
            Price = 0,
            Purchasable = true,
            RarityId = rarityId
        });

        var userId = Guid.NewGuid();

        db.DuckOwnerships.Add(new DuckOwnership
        {
            UserId = userId,
            ItemId = algoduckItemId,
            SelectedAsAvatar = false,
            SelectedForPond = false
        });

        await db.SaveChangesAsync();

        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "u@u.com", EmailConfirmed = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.IsInRoleAsync(user, "admin")).ReturnsAsync(false);

        var svc = new DefaultDuckService(db, userManager.Object);

        await svc.EnsureAlgoduckOwnedAndSelectedAsync(userId, CancellationToken.None);

        var ownership = await db.DuckOwnerships.SingleAsync(o => o.UserId == userId && o.ItemId == algoduckItemId);
        Assert.True(ownership.SelectedAsAvatar);
        Assert.True(ownership.SelectedForPond);

        var count = await db.DuckOwnerships.CountAsync(o => o.UserId == userId && o.ItemId == algoduckItemId);
        Assert.Equal(1, count);

        var selectedCount = await db.DuckOwnerships.CountAsync(o => o.UserId == userId && o.SelectedAsAvatar);
        Assert.Equal(1, selectedCount);
    }
}
