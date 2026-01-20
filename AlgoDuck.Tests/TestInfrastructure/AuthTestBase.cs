using System.Security.Cryptography;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.TestInfrastructure;

public abstract class AuthTestBase
{
    protected ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationCommandDbContext(options);
    }

    protected JwtSettings CreateJwtSettings()
    {
        return new JwtSettings
        {
            Issuer = "algoduck-tests",
            Audience = "algoduck-client",
            SigningKey = GenerateSigningKey(),
            AccessTokenMinutes = 15,
            RefreshTokenMinutes = 60
        };
    }

    protected JwtTokenProvider CreateJwtTokenProvider()
    {
        var options = Options.Create(CreateJwtSettings());
        return new JwtTokenProvider(options);
    }

    protected ApplicationUser CreateUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = "alice@gmail.com",
            Experience = 420,
            AmountSolved = 37,
            Coins = 100
        };
    }

    protected CancellationToken CreateCancellationToken()
    {
        return CancellationToken.None;
    }

    static string GenerateSigningKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}