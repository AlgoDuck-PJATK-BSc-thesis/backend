using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Models;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class PasswordResetTests
{
    readonly ApiCollectionFixture _fx;

    public PasswordResetTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task RequestReset_SendsEmail_AndResetAllowsLoginWithNewPassword()
    {
        var outbox = _fx.Scope.ServiceProvider.GetRequiredService<FakeEmailTransport>();
        outbox.Clear();

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"pwdreset_{suffix}@test.local";
        var username = $"pwdreset_{suffix}";
        var oldPassword = "Test1234!";
        var newPassword = "NewTest1234!";

        var seedClient = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, seedClient, email, username, oldPassword, "User", CancellationToken.None);

        var requestClient = _fx.CreateAnonymousClient();

        var requestResp = await requestClient.PostAsJsonAsync(
            "/api/auth/password-reset/request",
            new { Email = email },
            CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, requestResp.StatusCode);
        Assert.True(requestResp.StatusCode == HttpStatusCode.OK || requestResp.StatusCode == HttpStatusCode.Accepted);

        var msg = await WaitForEmailAsync(outbox, email, TimeSpan.FromSeconds(3), CancellationToken.None);
        Assert.NotNull(msg);

        var link = ExtractFirstLink(msg.TextBody);
        Assert.False(string.IsNullOrWhiteSpace(link));

        var userManager = _fx.Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var resetResp = await requestClient.PostAsJsonAsync(
            "/api/auth/password-reset/reset",
            new
            {
                UserId = user.Id,
                Token = token,
                Password = newPassword,
                ConfirmPassword = newPassword
            },
            CancellationToken.None);

        if (!(resetResp.StatusCode == HttpStatusCode.OK || resetResp.StatusCode == HttpStatusCode.NoContent))
        {
            var body = await resetResp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Reset expected 200 OK or 204 NoContent but got {(int)resetResp.StatusCode} {resetResp.StatusCode}. Body: {body}");
        }

        var loginClient = _fx.CreateAnonymousClient();
        await LoginWithoutSeedingAsync(loginClient, email, newPassword, CancellationToken.None);
    }

    static async Task<CapturedEmail?> WaitForEmailAsync(FakeEmailTransport outbox, string to, TimeSpan timeout, CancellationToken ct)
    {
        var stopAt = DateTimeOffset.UtcNow + timeout;

        while (DateTimeOffset.UtcNow < stopAt)
        {
            ct.ThrowIfCancellationRequested();

            var last = outbox.FindLastTo(to);
            if (last is not null)
            {
                return last;
            }

            await Task.Delay(50, ct);
        }

        return null;
    }

    static string? ExtractFirstLink(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }

        return null;
    }

    static async Task LoginWithoutSeedingAsync(HttpClient client, string userNameOrEmail, string password, CancellationToken ct)
    {
        var payload = new
        {
            UserNameOrEmail = userNameOrEmail,
            Password = password,
            RememberMe = true
        };

        var resp = await client.PostAsJsonAsync("/api/auth/login", payload, ct);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new Xunit.Sdk.XunitException($"Login expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }

        if (!resp.Headers.TryGetValues(HeaderNames.SetCookie, out var setCookies))
        {
            return;
        }

        var pairs = new List<string>();

        foreach (var sc in setCookies)
        {
            var firstPart = sc.Split(';', 2, StringSplitOptions.TrimEntries)[0];
            if (!string.IsNullOrWhiteSpace(firstPart))
            {
                pairs.Add(firstPart);
            }
        }

        var cookieHeader = string.Join("; ", pairs);

        client.DefaultRequestHeaders.Remove(HeaderNames.Cookie);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeader);
    }
}
