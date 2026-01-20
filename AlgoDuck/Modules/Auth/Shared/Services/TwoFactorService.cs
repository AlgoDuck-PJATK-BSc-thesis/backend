using System.Globalization;
using System.Security.Cryptography;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class TwoFactorService : ITwoFactorService
{
    private sealed record Entry(Guid UserId, string HashB64, string SaltB64, DateTimeOffset ExpiresAt, int Attempts);

    private const int CodeDigits = 6;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache _cache;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(
        IMemoryCache cache,
        IEmailSender emailSender,
        ILogger<TwoFactorService> logger)
    {
        _cache = cache;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<(string challengeId, DateTimeOffset expiresAt)> SendLoginCodeAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new TwoFactorException("User has no email address.");
        }

        var maxExclusive = (int)Math.Pow(10, CodeDigits);
        var code = RandomNumberGenerator.GetInt32(0, maxExclusive)
            .ToString("D" + CodeDigits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        var salt = HashingHelper.GenerateSalt();
        var hashB64 = HashingHelper.HashPassword(code, salt);
        var saltB64 = Convert.ToBase64String(salt);

        var challengeId = Guid.NewGuid().ToString("n");
        var expiresAt = DateTimeOffset.UtcNow.Add(CodeTtl);

        var entry = new Entry(user.Id, hashB64, saltB64, expiresAt, 0);
        _cache.Set(Key(challengeId), entry, expiresAt);

        await _emailSender.SendTwoFactorCodeAsync(user.Id, user.Email, code, cancellationToken);

        _logger.LogInformation("2FA code sent to user {UserId}", user.Id);

        return (challengeId, expiresAt);
    }

    public Task<(bool ok, Guid userId, string? error)> VerifyLoginCodeAsync(string challengeId, string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_cache.TryGetValue<Entry>(Key(challengeId), out var entry) || entry is null)
        {
            return Task.FromResult<(bool ok, Guid userId, string? error)>((false, Guid.Empty, "challenge_not_found"));
        }

        var now = DateTimeOffset.UtcNow;
        if (now > entry.ExpiresAt)
        {
            _cache.Remove(Key(challengeId));
            return Task.FromResult<(bool ok, Guid userId, string? error)>((false, Guid.Empty, "code_expired"));
        }

        var saltBytes = Convert.FromBase64String(entry.SaltB64);
        var computedHash = HashingHelper.HashPassword(code, saltBytes);

        if (!SlowEquals(computedHash, entry.HashB64))
        {
            var updated = entry with { Attempts = entry.Attempts + 1 };
            _cache.Set(Key(challengeId), updated, entry.ExpiresAt);

            if (updated.Attempts >= MaxAttempts)
            {
                _cache.Remove(Key(challengeId));
            }

            return Task.FromResult<(bool ok, Guid userId, string? error)>((false, Guid.Empty, "invalid_code"));
        }

        _cache.Remove(Key(challengeId));
        return Task.FromResult<(bool ok, Guid userId, string? error)>((true, entry.UserId, null));
    }

    private static string Key(string challengeId) => $"2fa:{challengeId}";

    private static bool SlowEquals(string aHashB64, string bHashB64)
    {
        var a = Convert.FromBase64String(aHashB64);
        var b = Convert.FromBase64String(bHashB64);

        var diff = (uint)a.Length ^ (uint)b.Length;
        var len = Math.Min(a.Length, b.Length);

        for (var i = 0; i < len; i++)
        {
            diff |= (uint)(a[i] ^ b[i]);
        }

        return diff == 0 && a.Length == b.Length;
    }
}