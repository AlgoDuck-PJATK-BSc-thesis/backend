using System.Security.Cryptography;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class ApiKeyGenerator
{
    private const int RawKeyBytesLength = 64;
    private const int PrefixLength = 16;

    public static ApiKeyMaterial Generate()
    {
        var rawBytes = RandomNumberGenerator.GetBytes(RawKeyBytesLength);
        var rawKey = Convert.ToBase64String(rawBytes);

        var prefix = rawKey.Length <= PrefixLength
            ? rawKey
            : rawKey.Substring(0, PrefixLength);

        var saltBytes = HashingHelper.GenerateSalt();
        var hashB64 = HashingHelper.HashPassword(rawKey, saltBytes);
        var saltB64 = Convert.ToBase64String(saltBytes);

        return new ApiKeyMaterial
        {
            RawKey = rawKey,
            Prefix = prefix,
            Hash = hashB64,
            Salt = saltB64
        };
    }
}

public sealed class ApiKeyMaterial
{
    public string RawKey { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public string Salt { get; init; } = string.Empty;
}