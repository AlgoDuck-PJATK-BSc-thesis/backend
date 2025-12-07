using System.Security.Cryptography;

namespace AlgoDuck.Modules.Auth.Shared.Jwt;

public sealed class TokenGenerator
{
    public string GenerateSecureToken(int sizeInBytes = 32)
    {
        var bytes = new byte[sizeInBytes];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string GenerateRefreshToken()
    {
        return GenerateSecureToken(32);
    }

    public string GenerateCsrfToken()
    {
        return GenerateSecureToken(16);
    }
}