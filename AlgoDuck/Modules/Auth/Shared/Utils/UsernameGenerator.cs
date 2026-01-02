using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class UsernameGenerator
{
    static readonly string[] Adjectives =
    [
        "brave","calm","clever","curious","daring","eager","fair","fast","gentle","happy",
        "kind","lucky","mighty","noble","polite","proud","quick","quiet","rapid","sharp",
        "smart","steady","strong","sunny","swift","tidy","witty","zany","bright","fuzzy",
        "crispy","cosmic","elegant","fierce","glossy","jolly","loyal","modern","neat","nimble",
        "patient","playful","pure","random","royal","silly","solid","super","tiny","vivid"
    ];

    static readonly string[] Nouns =
    [
        "alpaca","ant","badger","bear","beaver","bison","camel","cat","cobra","crow",
        "deer","dingo","dolphin","duck","eagle","falcon","fox","frog","gecko","goose",
        "heron","ibis","jaguar","koala","lemur","lion","lynx","marten","moose","otter",
        "panda","panther","pelican","puma","rabbit","raven","rhino","shark","sloth","squid",
        "tiger","turkey","turtle","walrus","whale","wolf","yak","zebra","orca","kestrel"
    ];

    public static async Task<string> GenerateUniqueDictionaryWithNumbersAsync<TUser>(
        UserManager<TUser> userManager,
        CancellationToken cancellationToken)
        where TUser : class
    {
        for (var attempt = 0; attempt < 400; attempt++)
        {
            var candidate = BuildDictionaryWithNumbersCandidate4();
            var existing = await userManager.FindByNameAsync(candidate);
            if (existing is null) return candidate;
        }

        for (var attempt = 0; attempt < 400; attempt++)
        {
            var candidate = BuildDictionaryWithNumbersCandidate6();
            var existing = await userManager.FindByNameAsync(candidate);
            if (existing is null) return candidate;
        }

        throw new InvalidOperationException("Could not generate unique username.");
    }

    public static async Task<string> GenerateUniqueAsync<TUser>(
        UserManager<TUser> userManager,
        string? displayName,
        string? email,
        CancellationToken cancellationToken)
        where TUser : class
    {
        var displayRaw = (displayName ?? string.Empty).Trim();
        var emailRaw = (email ?? string.Empty).Trim();

        var seedFromName = Normalize(displayRaw);
        var normalizedEmail = Normalize(emailRaw);

        var seeds = new List<string>();

        if (IsValidSeed(seedFromName) && !LooksLikeEmailSeed(displayRaw, emailRaw, seedFromName, normalizedEmail))
        {
            seeds.Add(TrimTo32(seedFromName));
        }

        for (var attempt = 0; attempt < 80; attempt++)
        {
            var candidate = BuildCandidate(seeds, attempt);
            var existing = await userManager.FindByNameAsync(candidate);
            if (existing is null) return candidate;
        }

        for (var attempt = 0; attempt < 200; attempt++)
        {
            var candidate = $"user_{RandomNumberGenerator.GetInt32(100000, 1000000)}";
            var existing = await userManager.FindByNameAsync(candidate);
            if (existing is null) return candidate;
        }

        throw new InvalidOperationException("Could not generate unique username.");
    }

    static string BuildDictionaryWithNumbersCandidate4()
    {
        var adj = Adjectives[RandomNumberGenerator.GetInt32(0, Adjectives.Length)];
        var noun = Nouns[RandomNumberGenerator.GetInt32(0, Nouns.Length)];
        var num = RandomNumberGenerator.GetInt32(1000, 10000);
        return TrimTo32($"{adj}_{noun}_{num}");
    }

    static string BuildDictionaryWithNumbersCandidate6()
    {
        var adj = Adjectives[RandomNumberGenerator.GetInt32(0, Adjectives.Length)];
        var noun = Nouns[RandomNumberGenerator.GetInt32(0, Nouns.Length)];
        var num = RandomNumberGenerator.GetInt32(100000, 1000000);
        return TrimTo32($"{adj}_{noun}_{num}");
    }

    static bool LooksLikeEmailSeed(string displayRaw, string emailRaw, string normalizedDisplay, string normalizedEmail)
    {
        if (string.IsNullOrWhiteSpace(displayRaw)) return false;
        if (displayRaw.Contains('@')) return true;

        if (!string.IsNullOrWhiteSpace(emailRaw))
        {
            if (string.Equals(displayRaw, emailRaw, StringComparison.OrdinalIgnoreCase)) return true;

            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                if (string.Equals(normalizedDisplay, normalizedEmail, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }

        return false;
    }

    static string BuildCandidate(IReadOnlyList<string> seeds, int attempt)
    {
        var adj = Adjectives[RandomNumberGenerator.GetInt32(0, Adjectives.Length)];
        var noun = Nouns[RandomNumberGenerator.GetInt32(0, Nouns.Length)];

        if (seeds.Count > 0 && attempt < 6)
        {
            var seed = seeds[0];

            if (attempt == 0) return TrimTo32(seed);
            if (attempt == 1) return TrimTo32($"{seed}_{noun}");
            if (attempt == 2) return TrimTo32($"{seed}_{adj}_{noun}");
            if (attempt == 3) return TrimTo32($"{seed}_{noun}_{RandomNumberGenerator.GetInt32(10, 10000)}");
            if (attempt == 4) return TrimTo32($"{seed}_{adj}_{noun}_{RandomNumberGenerator.GetInt32(10, 10000)}");
        }

        if (attempt % 3 == 0)
        {
            return TrimTo32($"{adj}_{noun}");
        }

        return TrimTo32($"{adj}_{noun}_{RandomNumberGenerator.GetInt32(10, 10000)}");
    }

    static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var sb = new StringBuilder(input.Length);
        var lastUnderscore = false;

        foreach (var ch in input.Trim().ToLowerInvariant())
        {
            var isAlphaNum = (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9');
            if (isAlphaNum)
            {
                sb.Append(ch);
                lastUnderscore = false;
                continue;
            }

            if (!lastUnderscore)
            {
                sb.Append('_');
                lastUnderscore = true;
            }
        }

        var s = sb.ToString().Trim('_');
        while (s.Contains("__", StringComparison.Ordinal)) s = s.Replace("__", "_", StringComparison.Ordinal);
        return s;
    }

    static bool IsValidSeed(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (s.Length < 3) return false;
        return true;
    }

    static string TrimTo32(string s)
    {
        if (s.Length <= 32) return s;
        return s[..32].TrimEnd('_');
    }
}
