using System.Net;
using AlgoDuck.Shared.Middleware;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

internal static class GeneralDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Configuration.AddEnvironmentVariables();

        var isDevLike = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing");

        var keysPath = "/var/app-keys";

        Console.WriteLine($"[DATA PROTECTION] Keys path: {keysPath}");
        Console.WriteLine($"[DATA PROTECTION] Directory exists: {Directory.Exists(keysPath)}");

        Directory.CreateDirectory(keysPath);

        Console.WriteLine($"[DATA PROTECTION] After create, exists: {Directory.Exists(keysPath)}");

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .SetApplicationName("AlgoDuck");

        builder.Services.AddMemoryCache();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                o.KnownNetworks.Clear();
                o.KnownProxies.Clear();
                o.ForwardLimit = 1;
                o.RequireHeaderSymmetry = false;
            });
        }
        else
        {
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                o.KnownProxies.Add(IPAddress.Loopback);
                o.KnownProxies.Add(IPAddress.IPv6Loopback);

                o.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.17.0.0"), 16));
                o.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 24));
                o.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.244.0.0"), 16));

                o.ForwardLimit = 1;
            });
        }

        builder.Services.Configure<SecurityHeadersOptions>(builder.Configuration.GetSection("SecurityHeaders"));

        if (builder.Environment.IsProduction())
        {
            builder.Services.AddHsts(o =>
            {
                o.Preload = true;
                o.IncludeSubDomains = true;
                o.MaxAge = TimeSpan.FromDays(365);
                o.ExcludedHosts.Add("localhost");
                o.ExcludedHosts.Add("127.0.0.1");
                o.ExcludedHosts.Add("[::1]");
            });
        }
    }
}
