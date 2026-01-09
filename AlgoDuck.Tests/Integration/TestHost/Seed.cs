using AlgoDuck.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoDuck.Tests.Integration.TestHost;

public sealed class Seed
{
    readonly IServiceProvider _services;

    public Seed(IServiceProvider services)
    {
        _services = services;
    }

    public async Task EnsureRoleAsync(string roleName, CancellationToken ct = default)
    {
        var roleManager = _services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var role = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<ApplicationUser> CreateUserAsync(string email, string username, string password, string roleName, bool emailConfirmed = true, CancellationToken ct = default)
    {
        var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = username,
            NormalizedUserName = username.ToUpperInvariant(),
            EmailConfirmed = emailConfirmed
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await EnsureRoleAsync(roleName, ct);

        var addRole = await userManager.AddToRoleAsync(user, roleName);
        if (!addRole.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", addRole.Errors.Select(e => e.Description)));
        }

        return user;
    }
}
