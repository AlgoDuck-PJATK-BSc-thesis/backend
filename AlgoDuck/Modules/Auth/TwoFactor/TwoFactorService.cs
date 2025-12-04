using System.Globalization;
using System.Security.Cryptography;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Email;
using Microsoft.Extensions.Caching.Memory;
using AlgoDuck.Shared.Utilities;


namespace AlgoDuck.Modules.Auth.TwoFactor
{
    public sealed class TwoFactorService : ITwoFactorService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailSender _email;
        private readonly ILogger<TwoFactorService> _log;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

        private sealed record Entry(Guid UserId, string HashB64, string SaltB64, DateTimeOffset ExpiresAt, int Attempts);

        public TwoFactorService(IMemoryCache cache, IEmailSender email, ILogger<TwoFactorService> log)
        {
            _cache = cache;
            _email = email;
            _log = log;
        }

        public async Task<(string challengeId, DateTimeOffset expiresAt)> SendLoginCodeAsync(ApplicationUser user, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(user.Email)) throw new InvalidOperationException("User has no email.");

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
            var salt = HashingHelper.GenerateSalt();
            var hashB64 = HashingHelper.HashPassword(code, salt);
            var saltB64 = Convert.ToBase64String(salt);

            var challengeId = Guid.NewGuid().ToString("n");
            var expires = DateTimeOffset.UtcNow.Add(_ttl);
            var entry = new Entry(user.Id, hashB64, saltB64, expires, 0);
            _cache.Set(Key(challengeId), entry, expires);

            var subject = "Your AlgoDuck sign-in code";
            var text = $"Your one-time code is {code}. It expires in 10 minutes.";
            var html = $"<p>Your one-time code is <b>{code}</b>.</p><p>It expires in 10 minutes.</p>";
            await _email.SendAsync(user.Email, subject, text, html, ct);

            _log.LogInformation("2FA code sent to user {UserId}", user.Id);
            return (challengeId, expires);
        }
        public Task<(bool ok, Guid userId, string? error)> VerifyLoginCodeAsync(string challengeId, string code, CancellationToken ct)
        {
            if (!_cache.TryGetValue<Entry>(Key(challengeId), out var entry) || entry is null)
                return Task.FromResult<(bool ok, Guid userId, string? error)>((false, Guid.Empty, "challenge_not_found"));

            if (DateTimeOffset.UtcNow > entry.ExpiresAt)
            {
                _cache.Remove(Key(challengeId));
                return Task.FromResult<(bool ok, Guid userId, string? error)>((false, Guid.Empty, "code_expired"));
            }

            var salt = Convert.FromBase64String(entry.SaltB64);
            var hashB64 = HashingHelper.HashPassword(code, salt);

            if (!SlowEquals(hashB64, entry.HashB64))
            {
                var updated = entry with { Attempts = entry.Attempts + 1 };
                _cache.Set(Key(challengeId), updated, entry.ExpiresAt);
                if (updated.Attempts >= 5) _cache.Remove(Key(challengeId));
                return Task.FromResult<(bool ok, Guid userId, string? error)>((false, Guid.Empty, "invalid_code"));
            }

            _cache.Remove(Key(challengeId));
            return Task.FromResult<(bool ok, Guid userId, string? error)>((true, entry.UserId, null));
        }

        private static string Key(string id) => $"2fa:{id}";

        private static bool SlowEquals(string a, string b)
        {
            var ba = Convert.FromBase64String(a);
            var bb = Convert.FromBase64String(b);
            var diff = (uint)ba.Length ^ (uint)bb.Length;
            var len = Math.Min(ba.Length, bb.Length);
            for (var i = 0; i < len; i++) diff |= (uint)(ba[i] ^ bb[i]);
            return diff == 0 && ba.Length == bb.Length;
        }
    }
}