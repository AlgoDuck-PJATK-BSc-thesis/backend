using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlgoDuck.Modules.Auth.Commands.Session.RevokeOtherSessions;
using AlgoDuck.Modules.Auth.Commands.Session.RevokeSession;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class SessionManagementTests
{
    readonly ApiCollectionFixture _fx;

    public SessionManagementTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task RevokeSession_WhenRevokingAnotherSession_ReturnsOk_AndRevokedSessionCannotRefresh()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];

        var email = $"revoke_session_{suffix}@test.local";
        var username = $"revoke_session_{suffix}";
        var password = "Test1234!";

        var clientA = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, clientA, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(clientA);

        var sessionA = await GetCurrentSessionIdAsync(clientA, CancellationToken.None);

        var clientB = _fx.CreateAnonymousClient();
        await LoginWithoutSeedingAsync(clientB, username, password, CancellationToken.None);
        SetCsrfHeaderFromCookie(clientB);

        var sessionB = await GetCurrentSessionIdAsync(clientB, CancellationToken.None);

        Assert.NotEqual(sessionA, sessionB);

        var revokeBody = new RevokeSessionDto { SessionId = sessionB };
        var revokeResp = await clientA.PostAsJsonAsync("/api/auth/sessions/revoke", revokeBody, CancellationToken.None);

        if (revokeResp.StatusCode != HttpStatusCode.OK)
        {
            var body = await revokeResp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Expected 200 OK but got {(int)revokeResp.StatusCode} {revokeResp.StatusCode}. Body: {body}");
        }

        var refreshB = await clientB.PostAsync("/api/auth/refresh", content: null, CancellationToken.None);

        Assert.True(refreshB.StatusCode == HttpStatusCode.Unauthorized || refreshB.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RevokeOtherSessions_WhenValidCurrentSession_ReturnsOk()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];

        var email = $"revoke_others_{suffix}@test.local";
        var username = $"revoke_others_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var currentSessionId = await GetCurrentSessionIdAsync(client, CancellationToken.None);

        var dto = new RevokeOtherSessionsDto { CurrentSessionId = currentSessionId };
        var resp = await client.PostAsJsonAsync("/api/auth/sessions/revoke-others", dto, CancellationToken.None);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var body = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }
    }

    static void SetCsrfHeaderFromCookie(HttpClient client)
    {
        var csrf = ExtractCookieValueFromClient(client, "csrf_token");
        Assert.False(string.IsNullOrWhiteSpace(csrf));

        client.DefaultRequestHeaders.Remove("X-CSRF-Token");
        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
    }

    static async Task<Guid> GetCurrentSessionIdAsync(HttpClient client, CancellationToken ct)
    {
        var resp = await client.GetAsync("/api/auth/sessions", ct);

        if (!resp.IsSuccessStatusCode)
        {
            var bad = await resp.Content.ReadAsStringAsync(ct);
            throw new Xunit.Sdk.XunitException($"GET /api/auth/sessions expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {bad}");
        }

        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var body = root.TryGetProperty("body", out var b) ? b : root;

        if (body.ValueKind == JsonValueKind.Object)
        {
            var direct =
                TryGetGuid(body, "currentSessionId") ??
                TryGetGuid(body, "CurrentSessionId") ??
                TryGetGuid(body, "sessionId") ??
                TryGetGuid(body, "SessionId");

            if (direct.HasValue && direct.Value != Guid.Empty)
            {
                return direct.Value;
            }

            if (body.TryGetProperty("sessions", out var sessionsEl) && sessionsEl.ValueKind == JsonValueKind.Array)
            {
                return ExtractCurrentFromSessionsArray(sessionsEl, json);
            }

            if (body.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
            {
                return ExtractCurrentFromSessionsArray(itemsEl, json);
            }
        }

        if (body.ValueKind == JsonValueKind.Array)
        {
            return ExtractCurrentFromSessionsArray(body, json);
        }

        throw new Xunit.Sdk.XunitException($"Could not discover current session id from /api/auth/sessions response. Body: {json}");
    }

    static Guid ExtractCurrentFromSessionsArray(JsonElement arr, string rawJson)
    {
        Guid? first = null;

        foreach (var item in arr.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;

            var sid =
                TryGetGuid(item, "sessionId") ??
                TryGetGuid(item, "SessionId") ??
                TryGetGuid(item, "id") ??
                TryGetGuid(item, "Id");

            if (!sid.HasValue || sid.Value == Guid.Empty) continue;

            first ??= sid.Value;

            var isCurrent =
                TryGetBool(item, "isCurrent") ??
                TryGetBool(item, "IsCurrent") ??
                TryGetBool(item, "isCurrentSession") ??
                TryGetBool(item, "IsCurrentSession");

            if (isCurrent == true)
            {
                return sid.Value;
            }
        }

        if (first.HasValue && first.Value != Guid.Empty)
        {
            return first.Value;
        }

        throw new Xunit.Sdk.XunitException($"Sessions list did not contain any session identifiers. Body: {rawJson}");
    }

    static bool? TryGetBool(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var el)) return null;

        if (el.ValueKind == JsonValueKind.True) return true;
        if (el.ValueKind == JsonValueKind.False) return false;

        if (el.ValueKind == JsonValueKind.String)
        {
            var s = el.GetString();
            if (bool.TryParse(s, out var b)) return b;
        }

        return null;
    }

    static Guid? TryGetGuid(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var el)) return null;

        if (el.ValueKind == JsonValueKind.String)
        {
            var s = el.GetString();
            if (Guid.TryParse(s, out var g)) return g;
            return null;
        }

        if (el.ValueKind == JsonValueKind.Undefined || el.ValueKind == JsonValueKind.Null) return null;

        try
        {
            return el.GetGuid();
        }
        catch
        {
            return null;
        }
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
            throw new Xunit.Sdk.XunitException("Login response did not contain Set-Cookie headers.");
        }

        var cookieHeader = BuildCookieHeader(setCookies);

        client.DefaultRequestHeaders.Remove(HeaderNames.Cookie);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeader);
    }

    static string BuildCookieHeader(IEnumerable<string> setCookieHeaders)
    {
        var pairs = new List<string>();

        foreach (var sc in setCookieHeaders)
        {
            var firstPart = sc.Split(';', 2, StringSplitOptions.TrimEntries)[0];
            if (!string.IsNullOrWhiteSpace(firstPart))
            {
                pairs.Add(firstPart);
            }
        }

        return string.Join("; ", pairs);
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
