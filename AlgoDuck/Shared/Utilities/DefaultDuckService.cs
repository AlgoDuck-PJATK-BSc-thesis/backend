using AlgoDuck.DAL;
using AlgoDuck.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Shared.Utilities;

public interface IDefaultDuckService
{
    Task EnsureAlgoduckOwnedAndSelectedAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed class DefaultDuckService : IDefaultDuckService
{
    private readonly ApplicationCommandDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DefaultDuckService(ApplicationCommandDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task EnsureAlgoduckOwnedAndSelectedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "admin");
        if (isAdmin)
        {
            return;
        }

        var algoduckItemId = await _db.Set<DuckItem>()
            .Where(i => i.Name.ToLower() == "algoduck")
            .Select(i => i.ItemId)
            .FirstOrDefaultAsync(cancellationToken);

        if (algoduckItemId == Guid.Empty)
        {
            throw new InvalidOperationException("Default duck item 'algoduck' was not found.");
        }

        var selected = await _db.Set<DuckOwnership>()
            .Where(o => o.UserId == userId && o.SelectedAsAvatar)
            .ToListAsync(cancellationToken);

        foreach (var s in selected)
        {
            s.SelectedAsAvatar = false;
        }

        var owned = await _db.Set<DuckOwnership>()
            .SingleOrDefaultAsync(o => o.UserId == userId && o.ItemId == algoduckItemId, cancellationToken);

        if (owned is null)
        {
            owned = new DuckOwnership
            {
                UserId = userId,
                ItemId = algoduckItemId,
                SelectedAsAvatar = true,
                SelectedForPond = true
            };
            _db.Set<DuckOwnership>().Add(owned);
        }
        else
        {
            owned.SelectedAsAvatar = true;
            owned.SelectedForPond = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
