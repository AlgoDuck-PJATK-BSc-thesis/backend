using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.SelectAvatar;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Commands.SelectAvatar;

public sealed class SelectAvatarHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        await using var dbContext = CreateCommandDbContext();

        var dto = new SelectAvatarDto { ItemId = Guid.NewGuid() };
        var validator = CreateValidatorMock(dto);

        var handler = new SelectAvatarHandler(dbContext, validator.Object);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNoPurchases_ThenThrowsUserNotFoundException()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);

        var dto = new SelectAvatarDto { ItemId = Guid.NewGuid() };
        var validator = CreateValidatorMock(dto);

        var handler = new SelectAvatarHandler(dbContext, validator.Object);

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("no purchases", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnItem_ThenThrowsValidationException()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        var ownedItemId = Guid.NewGuid();
        var requestedItemId = Guid.NewGuid();

        var user = SeedUser(dbContext, userId);
        var rarity = SeedRarity(dbContext, Guid.NewGuid());
        var ownedItem = SeedItem(dbContext, ownedItemId, rarity.RarityId, rarity);

        SeedPurchase(dbContext, user, ownedItem, false);

        var dto = new SelectAvatarDto { ItemId = requestedItemId };
        var validator = CreateValidatorMock(dto);

        var handler = new SelectAvatarHandler(dbContext, validator.Object);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("does not own", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenSelectsOnlyRequestedPurchase()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();

        var user = SeedUser(dbContext, userId);
        var rarity = SeedRarity(dbContext, Guid.NewGuid());

        var item1 = SeedItem(dbContext, item1Id, rarity.RarityId, rarity);
        var item2 = SeedItem(dbContext, item2Id, rarity.RarityId, rarity);

        var p1 = SeedPurchase(dbContext, user, item1, true);
        var p2 = SeedPurchase(dbContext, user, item2, false);

        var dto = new SelectAvatarDto { ItemId = item2Id };
        var validator = CreateValidatorMock(dto);

        var handler = new SelectAvatarHandler(dbContext, validator.Object);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.False(p1.SelectedAsAvatar);
        Assert.True(p2.SelectedAsAvatar);
    }

    static ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static Mock<IValidator<SelectAvatarDto>> CreateValidatorMock(SelectAvatarDto dto)
    {
        var mock = new Mock<IValidator<SelectAvatarDto>>();
        mock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    static ApplicationUser SeedUser(ApplicationCommandDbContext dbContext, Guid userId)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"user_{userId:N}",
            Email = $"user_{userId:N}@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        dbContext.ApplicationUsers.Add(user);
        dbContext.SaveChanges();

        return user;
    }

    static Rarity SeedRarity(ApplicationCommandDbContext dbContext, Guid rarityId)
    {
        var rarity = new Rarity
        {
            RarityId = rarityId,
            RarityName = "Common"
        };

        dbContext.Rarities.Add(rarity);
        dbContext.SaveChanges();

        return rarity;
    }

    static Models.Item SeedItem(ApplicationCommandDbContext dbContext, Guid itemId, Guid rarityId, Rarity rarity)
    {
        var item = new DuckItem
        {
            ItemId = itemId,
            Name = $"Item_{itemId:N}",
            Description = null,
            Price = 1,
            Purchasable = true,
            RarityId = rarityId,
            Rarity = rarity
        };

        dbContext.Items.Add(item);
        dbContext.SaveChanges();

        return item;
    }

    static DuckOwnership SeedPurchase(ApplicationCommandDbContext dbContext, ApplicationUser user, Models.Item item, bool selected)
    {
        var purchase = new DuckOwnership()
        {
            UserId = user.Id,
            ItemId = item.ItemId,
            SelectedAsAvatar = selected,
            User = user,
            Item = item
        };

        dbContext.Purchases.Add(purchase);
        dbContext.SaveChanges();

        return purchase;
    }
}