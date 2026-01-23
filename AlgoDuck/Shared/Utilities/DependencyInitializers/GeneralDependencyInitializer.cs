using System.Net;
using AlgoDuck.Shared.Middleware;
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

        var configuredKeysPath =
            builder.Configuration["APP_KEYS_DIR"] ??
            builder.Configuration["App:KeysDir"] ??
            builder.Configuration["AppKeys:Directory"];

        var runningInContainer = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

        string keysPath;

        if (!string.IsNullOrWhiteSpace(configuredKeysPath))
        {
            keysPath = configuredKeysPath;
        }
        else if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsEnvironment("Test"))
        {
            keysPath = Path.Combine(Path.GetTempPath(), "algoduck", "app-keys");
        }
        else if (builder.Environment.IsDevelopment())
        {
            keysPath = runningInContainer
                ? "/var/app-keys"
                : Path.Combine(builder.Environment.ContentRootPath, "app-keys");
        }
        else
        {
            keysPath = "/var/app-keys";
        }

        Console.WriteLine($"[DATA PROTECTION] Keys path: {keysPath}");
        Console.WriteLine($"[DATA PROTECTION] Directory exists: {Directory.Exists(keysPath)}");

        try
        {
            Directory.CreateDirectory(keysPath);
        }
        catch (UnauthorizedAccessException)
        {
            var fallback = Path.Combine(Path.GetTempPath(), "algoduck", "app-keys");
            Directory.CreateDirectory(fallback);
            keysPath = fallback;
        }
        catch (IOException)
        {
            var fallback = Path.Combine(Path.GetTempPath(), "algoduck", "app-keys");
            Directory.CreateDirectory(fallback);
            keysPath = fallback;
        }

        Console.WriteLine($"[DATA PROTECTION] After create, exists: {Directory.Exists(keysPath)}");
        Console.WriteLine($"[DATA PROTECTION] Final keys path: {keysPath}");

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