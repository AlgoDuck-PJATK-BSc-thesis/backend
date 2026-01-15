using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class EmailVerificationStartTests
{
    readonly ApiCollectionFixture _fx;

    public EmailVerificationStartTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task EmailVerificationStart_WhenCalled_SendsEmailWithLink()
    {
        var outbox = _fx.Scope.ServiceProvider.GetRequiredService<FakeEmailTransport>();
        outbox.Clear();

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"emailverify_start_{suffix}@test.local";
        var username = $"emailverify_start_{suffix}";
        var password = "Test1234!";

        var seed = new Seed(_fx.Scope.ServiceProvider);
        await seed.CreateUserAsync(email, username, password, "User", emailConfirmed: false, CancellationToken.None);

        var client = _fx.CreateAnonymousClient();

        var resp = await client.PostAsJsonAsync(
            "/api/auth/email-verification/start",
            new { Email = email },
            CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.True(resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted);

        var msg = await WaitForEmailAsync(outbox, email, TimeSpan.FromSeconds(3), CancellationToken.None);
        Assert.NotNull(msg);

        var link = ExtractFirstLink(msg.TextBody);
        Assert.False(string.IsNullOrWhiteSpace(link));
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
}
