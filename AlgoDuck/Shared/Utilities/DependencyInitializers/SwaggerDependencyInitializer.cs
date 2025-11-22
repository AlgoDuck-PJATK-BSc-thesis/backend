using Microsoft.OpenApi.Models;

namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

public class SwaggerDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        
        builder.Services.AddEndpointsApiExplorer();
        
        var csrfHeaderName = builder.Configuration.GetSection("Jwt").GetValue<string>("CsrfHeaderName") ?? "X-CSRF-Token";

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AlgoDuck API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Optional: Bearer token (not used by cookie auth)"
            });

            c.AddSecurityDefinition("CsrfToken", new OpenApiSecurityScheme
            {
                Name = csrfHeaderName,
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "CSRF token matching the CSRF cookie"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CsrfToken" }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

}