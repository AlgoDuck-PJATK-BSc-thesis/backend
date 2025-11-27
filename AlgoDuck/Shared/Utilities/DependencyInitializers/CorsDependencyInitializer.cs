namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

internal static class CorsDependencyInitializer
{
    
    
    internal static void Initialize(WebApplicationBuilder builder)
    {
        
        var devOrigins = builder.Configuration.GetSection("Cors:DevOrigins").Get<string[]>() 
                         ?? ["http://localhost:5173", "https://localhost:5173"];
        var prodOrigins = builder.Configuration.GetSection("Cors:ProdOrigins").Get<string[]>() ?? [];
        
        if (builder.Environment.IsProduction() && prodOrigins.Length == 0)
            throw new InvalidOperationException("Cors:ProdOrigins must be configured in Production.");
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
            {
                policy.WithOrigins(devOrigins)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Token-Expired");
            });

            options.AddPolicy("ProdCors", policy =>
            {
                policy.WithOrigins(prodOrigins)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Token-Expired");
            });
        });
    }
}