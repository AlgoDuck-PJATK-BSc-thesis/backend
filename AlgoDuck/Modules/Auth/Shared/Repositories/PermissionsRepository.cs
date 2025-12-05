using System.Security.Claims;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Shared.Repositories;

public sealed class PermissionsRepository : IPermissionsRepository
{
    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionsRepository(
        ApplicationCommandDbContext commandDbContext,
        UserManager<ApplicationUser> userManager)
    {
        _commandDbContext = commandDbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _commandDbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            permissions.Add("role:" + role);
        }

        foreach (var claim in claims)
        {
            if (string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            {
                permissions.Add("role:" + claim.Value);
            }
            else if (string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase))
            {
                permissions.Add(claim.Value);
            }
        }

        return permissions.ToList();
    }

    public async Task<IReadOnlyList<Claim>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _commandDbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Array.Empty<Claim>();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        return claims.ToList();
    }
}