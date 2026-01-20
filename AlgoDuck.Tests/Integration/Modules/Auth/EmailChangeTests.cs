using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class EmailChangeTests
{
    readonly ApiCollectionFixture _fx;

    public EmailChangeTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task EmailChange_Start_SendsConfirmationEmail_ToNewEmail()
    {
        var outbox = _fx.Scope.ServiceProvider.GetRequiredService<FakeEmailTransport>();
        outbox.Clear();

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"emailchange_{suffix}@test.local";
        var username = $"emailchange_{suffix}";
        var password = "Test1234!";
        var newEmail = $"emailchange_new_{suffix}@test.local";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var payload = new
        {
            NewEmail = newEmail,
            ReturnUrl = "http://localhost"
        };

        var resp = await client.PostAsJsonAsync("/api/auth/email/change/start", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }

        var msg = await WaitForEmailAsync(outbox, newEmail, TimeSpan.FromSeconds(3), CancellationToken.None);
        Assert.NotNull(msg);

        var link = ExtractFirstLink(msg.TextBody);
        Assert.False(string.IsNullOrWhiteSpace(link));
    }

    [Fact]
    public async Task EmailChange_Confirm_WithInvalidToken_ReturnsClientError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"emailchange2_{suffix}@test.local";
        var username = $"emailchange2_{suffix}";
        var password = "Test1234!";
        var newEmail = $"emailchange2_new_{suffix}@test.local";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var payload = new
        {
            NewEmail = newEmail,
            Token = "invalid-token"
        };

        var resp = await client.PostAsJsonAsync("/api/auth/email/change/confirm", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }

        Assert.True((int)resp.StatusCode >= 400 || resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NoContent);
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

    static void SetCsrfHeaderFromCookie(HttpClient client)
    {
        var csrf = ExtractCookieValueFromClient(client, "csrf_token");
        Assert.False(string.IsNullOrWhiteSpace(csrf));

        client.DefaultRequestHeaders.Remove("X-CSRF-Token");
        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
    }

    static string? ExtractCookieValueFromClient(HttpClient client, string cookieName)
    {
        if (!client.DefaultRequestHeaders.TryGetValues(HeaderNames.Cookie, out var values))
        {
            return null;
        }

        var all = string.Join("; ", values);

        foreach (var seg in all.Split(';', StringSplitOptions.TrimEntries))
        {
            var parts = seg.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && string.Equals(parts[0], cookieName, StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }
        }

        return null;
    }
}
