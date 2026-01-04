using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Queries.Identity.GetUserByToken;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Queries.GetUserByToken;

public sealed class GetUserByTokenHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>> { new Mock<IUserValidator<ApplicationUser>>().Object };
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new Mock<IPasswordValidator<ApplicationUser>>().Object };
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            logger.Object
        );
    }

    static JwtTokenProvider CreateProvider(string signingKey)
    {
        return new JwtTokenProvider(Options.Create(new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = signingKey,
            AccessTokenMinutes = 15
        }));
    }

    static string CreateToken(string signingKey, string subValue, DateTimeOffset notBefore, DateTimeOffset expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subValue),
            new(ClaimTypes.NameIdentifier, subValue),
            new(ClaimTypes.Name, "alice"),
            new(JwtRegisteredClaimNames.Email, "alice@example.com")
        };

        var jwt = new JwtSecurityToken(
            issuer: "issuer",
            audience: "audience",
            claims: claims,
            notBefore: notBefore.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    [Fact]
    public async Task HandleAsync_WhenAccessTokenEmpty_ReturnsNull()
    {
        var handler = new GetUserByTokenHandler(CreateProvider(new string('k', 64)), CreateUserManagerMock().Object);

        var result = await handler.HandleAsync(new UserByTokenDto { AccessToken = "" }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenExpired_ReturnsNull()
    {
        var signingKey = new string('k', 64);
        var provider = CreateProvider(signingKey);
        var userManager = CreateUserManagerMock();
        var handler = new GetUserByTokenHandler(provider, userManager.Object);

        var token = CreateToken(signingKey, Guid.NewGuid().ToString(), DateTimeOffset.UtcNow.AddMinutes(-10), DateTimeOffset.UtcNow.AddMinutes(-5));

        var result = await handler.HandleAsync(new UserByTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.Null(result);
        userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenSignatureInvalid_ReturnsNull()
    {
        var provider = CreateProvider(new string('a', 64));
        var userManager = CreateUserManagerMock();
        var handler = new GetUserByTokenHandler(provider, userManager.Object);

        var tokenSignedWithDifferentKey = CreateToken(new string('b', 64), Guid.NewGuid().ToString(), DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));

        var result = await handler.HandleAsync(new UserByTokenDto { AccessToken = tokenSignedWithDifferentKey }, CancellationToken.None);

        Assert.Null(result);
        userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdClaimNotGuid_ReturnsNull()
    {
        var signingKey = new string('k', 64);
        var provider = CreateProvider(signingKey);
        var userManager = CreateUserManagerMock();
        var handler = new GetUserByTokenHandler(provider, userManager.Object);

        var token = CreateToken(signingKey, "not-a-guid", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));

        var result = await handler.HandleAsync(new UserByTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.Null(result);
        userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValidTokenAndUserExists_ReturnsAuthUserDto()
    {
        var signingKey = new string('k', 64);
        var provider = CreateProvider(signingKey);
        var userManager = CreateUserManagerMock();
        var handler = new GetUserByTokenHandler(provider, userManager.Object);

        var userId = Guid.NewGuid();
        var token = provider.CreateAccessToken(new ApplicationUser { Id = userId, UserName = "alice", Email = "alice@example.com" }, Guid.NewGuid(), out _);

        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                UserName = "alice",
                Email = "alice@example.com",
                EmailConfirmed = true
            });

        var result = await handler.HandleAsync(new UserByTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.Equal("alice", result.UserName);
        Assert.Equal("alice@example.com", result.Email);
        Assert.True(result.EmailConfirmed);
    }

    [Fact]
    public async Task HandleAsync_WhenValidTokenButUserNotFound_ReturnsNull()
    {
        var signingKey = new string('k', 64);
        var provider = CreateProvider(signingKey);
        var userManager = CreateUserManagerMock();
        var handler = new GetUserByTokenHandler(provider, userManager.Object);

        var userId = Guid.NewGuid();
        var token = provider.CreateAccessToken(new ApplicationUser { Id = userId, UserName = "alice", Email = "alice@example.com" }, Guid.NewGuid(), out _);

        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await handler.HandleAsync(new UserByTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.Null(result);
        userManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
    }
}
