using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Mappers;

public static class AuthMapper
{
    public static AuthResponse ToAuthResponse(
        ApplicationUser user,
        Session session,
        string accessToken,
        string refreshToken,
        string csrfToken,
        DateTimeOffset accessTokenExpiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            CsrfToken = csrfToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            SessionId = session.SessionId,
            UserId = user.Id
        };
    }

    public static RefreshResult ToRefreshResult(
        Session session,
        string accessToken,
        string refreshToken,
        string csrfToken,
        DateTimeOffset accessTokenExpiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        return new RefreshResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            CsrfToken = csrfToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            SessionId = session.SessionId,
            UserId = session.UserId
        };
    }
}