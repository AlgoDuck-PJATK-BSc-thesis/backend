using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.VerifyTwoFactorLogin;

public sealed class VerifyTwoFactorLoginHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new VerifyTwoFactorLoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new VerifyTwoFactorLoginValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new VerifyTwoFactorLoginDto { ChallengeId = "", Code = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenVerificationNotOk_ThrowsTwoFactorException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new VerifyTwoFactorLoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new VerifyTwoFactorLoginValidator());

        var dto = new VerifyTwoFactorLoginDto { ChallengeId = "c", Code = "123456" };

        twoFactorService
            .Setup(x => x.VerifyLoginCodeAsync(dto.ChallengeId, dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, Guid.Empty, "bad"));

        var ex = await Assert.ThrowsAsync<TwoFactorException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("two_factor_error", ex.Code);
        Assert.Equal("bad", ex.Message);
    }
}
