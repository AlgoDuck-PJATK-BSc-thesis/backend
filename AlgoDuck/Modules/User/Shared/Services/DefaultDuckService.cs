using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

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

        var hasAnySelectedAvatar = await _db.Set<DuckOwnership>()
            .AnyAsync(o => o.UserId == userId && o.SelectedAsAvatar, cancellationToken);

        var ownedAlgoduck = await _db.Set<DuckOwnership>()
            .SingleOrDefaultAsync(o => o.UserId == userId && o.ItemId == algoduckItemId, cancellationToken);

        if (ownedAlgoduck is null)
        {
            ownedAlgoduck = new DuckOwnership
            {
                UserId = userId,
                ItemId = algoduckItemId,
                SelectedAsAvatar = false,
                SelectedForPond = true
            };
            _db.Set<DuckOwnership>().Add(ownedAlgoduck);
        }
        else
        {
            ownedAlgoduck.SelectedForPond = true;
        }

        if (!hasAnySelectedAvatar)
        {
            var selected = await _db.Set<DuckOwnership>()
                .Where(o => o.UserId == userId && o.SelectedAsAvatar)
                .ToListAsync(cancellationToken);

            foreach (var s in selected)
            {
                s.SelectedAsAvatar = false;
            }

            ownedAlgoduck.SelectedAsAvatar = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
