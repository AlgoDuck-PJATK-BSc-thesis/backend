using AlgoDuck.Modules.Auth.Queries.CheckUserPermissions;
using AlgoDuck.Modules.Auth.Queries.GetApiKeys;
using AlgoDuck.Modules.Auth.Queries.GetCurrentUser;
using AlgoDuck.Modules.Auth.Queries.GetUserByToken;
using AlgoDuck.Modules.Auth.Queries.GetUserSessions;
using AlgoDuck.Modules.Auth.Queries.SearchUsersByEmail;
using AlgoDuck.Modules.Auth.Queries.ValidateToken;
using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries;

public static class AuthQueriesDependencyInitializer
{
    public static IServiceCollection AddAuthQueries(this IServiceCollection services)
    {
        services.AddScoped<IValidator<Guid>, GetUserSessionsValidator>();
        services.AddScoped<IGetUserSessionsHandler, GetUserSessionsHandler>();

        services.AddScoped<IValidator<Guid>, GetApiKeysValidator>();
        services.AddScoped<IGetApiKeysHandler, GetApiKeysHandler>();

        services.AddScoped<IValidator<Guid>, GetCurrentUserValidator>();
        services.AddScoped<IGetCurrentUserHandler, GetCurrentUserHandler>();

        services.AddScoped<IValidator<ValidateTokenDto>, ValidateTokenValidator>();
        services.AddScoped<IValidateTokenHandler, ValidateTokenHandler>();

        services.AddScoped<IValidator<UserByTokenDto>, GetUserByTokenValidator>();
        services.AddScoped<IGetUserByTokenHandler, GetUserByTokenHandler>();

        services.AddScoped<IValidator<PermissionsDto>, CheckUserPermissionsValidator>();
        services.AddScoped<ICheckUserPermissionsHandler, CheckUserPermissionsHandler>();

        services.AddScoped<IValidator<SearchUsersByEmailDto>, SearchUsersByEmailValidator>();
        services.AddScoped<ISearchUsersByEmailHandler, SearchUsersByEmailHandler>();

        return services;
    }
}