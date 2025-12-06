using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.SelectAvatar;

public sealed class SelectAvatarHandler : ISelectAvatarHandler
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IValidator<SelectAvatarDto> _validator;

    public SelectAvatarHandler(
        ApplicationCommandDbContext dbContext,
        IValidator<SelectAvatarDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, SelectAvatarDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("User identifier is invalid.");
        }

        var purchases = await _dbContext.Purchases
            .Include(p => p.Item)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        if (purchases.Count == 0)
        {
            throw new UserNotFoundException("User has no purchases.");
        }

        var selectedPurchase = purchases.FirstOrDefault(p => p.ItemId == dto.ItemId);
        if (selectedPurchase is null)
        {
            throw new Shared.Exceptions.ValidationException("User does not own this item.");
        }

        foreach (var purchase in purchases)
        {
            purchase.Selected = purchase == selectedPurchase;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}