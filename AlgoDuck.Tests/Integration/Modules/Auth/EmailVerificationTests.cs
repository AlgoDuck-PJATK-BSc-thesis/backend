using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class EmailVerificationTests
{
    readonly ApiCollectionFixture _fx;

    public EmailVerificationTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Register_SendsConfirmationEmail_AndVerifyEndpointAcceptsToken()
    {
        var outbox = _fx.Scope.ServiceProvider.GetRequiredService<FakeEmailTransport>();
        outbox.Clear();

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"emailverify_{suffix}@test.local";
        var username = $"emailverify_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();

        var registerPayload = new
        {
            UserName = username,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        var registerResp = await client.PostAsJsonAsync("/api/auth/register", registerPayload, CancellationToken.None);
        EnsureOkOrCreatedOrThrow(registerResp);

        var msg = await WaitForEmailAsync(outbox, email, TimeSpan.FromSeconds(3), CancellationToken.None);
        Assert.NotNull(msg);

        var link = ExtractFirstLink(msg.TextBody);
        Assert.False(string.IsNullOrWhiteSpace(link));

        var uri = new Uri(link, UriKind.Absolute);

        var verifyResp = await client.GetAsync(uri.PathAndQuery, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, verifyResp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, verifyResp.StatusCode);

        Assert.True(
            verifyResp.StatusCode == HttpStatusCode.OK ||
            verifyResp.StatusCode == HttpStatusCode.NoContent ||
            verifyResp.StatusCode == HttpStatusCode.Found ||
            verifyResp.StatusCode == HttpStatusCode.Redirect ||
            verifyResp.StatusCode == HttpStatusCode.RedirectMethod ||
            verifyResp.StatusCode == HttpStatusCode.TemporaryRedirect ||
            (int)verifyResp.StatusCode == 308);
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

    static void EnsureOkOrCreatedOrThrow(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
        {
            return;
        }

        var body = resp.Content.ReadAsStringAsync(CancellationToken.None).GetAwaiter().GetResult();
        throw new Xunit.Sdk.XunitException($"Expected 200 OK or 201 Created but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
    }
}
