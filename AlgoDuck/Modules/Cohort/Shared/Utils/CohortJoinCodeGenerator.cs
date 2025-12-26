using System.Security.Cryptography;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;

namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public static class CohortJoinCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static async Task<string> GenerateUniqueAsync(
        ICohortRepository cohortRepository,
        int length,
        CancellationToken cancellationToken)
    {
        if (length < 6) length = 6;
        if (length > 16) length = 16;

        for (var attempt = 0; attempt < 50; attempt++)
        {
            var code = Generate(length);

            if (!await cohortRepository.JoinCodeExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate unique cohort join code.");
    }

    private static string Generate(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}