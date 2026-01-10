using System.Net;
using System.Net.Http.Json;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.TestHost;

internal static class AuthFlow
{
    public static async Task LoginAsync(ApiCollectionFixture fx, HttpClient client, string email, string username, string password, string role, CancellationToken ct)
    {
        var seed = new Seed(fx.Scope.ServiceProvider);
        await seed.CreateUserAsync(email, username, password, role, true, ct);

        var payload = new Dictionary<string, object?>
        {
            ["userNameOrEmail"] = email,
            ["UserNameOrEmail"] = email,
            ["password"] = password,
            ["Password"] = password
        };

        var resp = await client.PostAsync("/api/auth/login", JsonContent.Create(payload), ct);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new Xunit.Sdk.XunitException($"Login failed: {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }

        if (!resp.Headers.TryGetValues(HeaderNames.SetCookie, out var setCookies))
        {
            return;
        }

        var cookieHeader = BuildCookieHeader(setCookies);
        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            client.DefaultRequestHeaders.Remove(HeaderNames.Cookie);
            client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeader);
        }
    }

    private static string BuildCookieHeader(IEnumerable<string> setCookieHeaders)
    {
        var parts = new List<string>();

        foreach (var sc in setCookieHeaders)
        {
            var first = sc.Split(';', 2, StringSplitOptions.TrimEntries)[0];
            if (!string.IsNullOrWhiteSpace(first))
            {
                parts.Add(first);
            }
        }

        return string.Join("; ", parts);
    }
}