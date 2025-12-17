namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

internal static class CorsDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        var devOrigins = builder.Configuration.GetSection("Cors:DevOrigins").Get<string[]>();
        var prodOrigins = builder.Configuration.GetSection("Cors:ProdOrigins").Get<string[]>();

        if (builder.Environment.IsDevelopment() && (devOrigins is null || devOrigins.Length == 0))
            throw new InvalidOperationException("Cors:DevOrigins must be configured (set CORS__DEVORIGINS__0, ...).");

        if (builder.Environment.IsProduction() && (prodOrigins is null || prodOrigins.Length == 0))
            throw new InvalidOperationException("Cors:ProdOrigins must be configured in Production.");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
            {
                policy.WithOrigins(devOrigins ?? Array.Empty<string>())
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Token-Expired", "X-Auth-Error");
            });

            options.AddPolicy("ProdCors", policy =>
            {
                policy.WithOrigins(prodOrigins ?? Array.Empty<string>())
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Token-Expired", "X-Auth-Error");
            });
        });
    }
}