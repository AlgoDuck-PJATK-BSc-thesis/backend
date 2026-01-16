using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoDuck.Tests.Integration.TestHost;

public static class EndpointCatalog
{
    public sealed record ApiEndpoint(
        string Route,
        IReadOnlyList<string> Methods,
        bool RequiresAuth,
        bool IsAdminOnly
    );

    public static IReadOnlyList<ApiEndpoint> GetApiEndpoints(IServiceProvider sp)
    {
        var sources = sp.GetServices<EndpointDataSource>().ToList();
        var list = new List<ApiEndpoint>();

        foreach (var source in sources)
        {
            foreach (var ep in source.Endpoints.OfType<RouteEndpoint>())
            {
                var raw = ep.RoutePattern.RawText ?? "";
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                if (!raw.StartsWith("/"))
                {
                    raw = "/" + raw;
                }

                if (!raw.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (raw.StartsWith("/api/hubs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (raw.Contains("{", StringComparison.Ordinal))
                {
                    continue;
                }

                var http = ep.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
                var methods = http?.HttpMethods.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();

                if (methods.Count == 0)
                {
                    continue;
                }

                var auth = ep.Metadata.OfType<IAuthorizeData>().ToList();
                var requiresAuth = auth.Count > 0;

                var roles = auth
                    .SelectMany(a => (a.Roles ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .ToList();

                var isAdminOnly =
                    roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)) ||
                    auth.Any(a => (a.Policy ?? "").Contains("Admin", StringComparison.OrdinalIgnoreCase));

                list.Add(new ApiEndpoint(raw, methods, requiresAuth, isAdminOnly));
            }
        }

        return list
            .GroupBy(e => (e.Route, string.Join(",", e.Methods.OrderBy(m => m, StringComparer.OrdinalIgnoreCase))))
            .Select(g => g.First())
            .OrderBy(e => e.Route, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static HttpRequestMessage CreateRequest(string method, string route)
    {
        return new HttpRequestMessage(new HttpMethod(method), route);
    }
}
