using System.Security.Cryptography;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        }

        var salt = HashingHelper.GenerateSalt();
        var hashB64 = HashingHelper.HashPassword(password, salt);
        var saltB64 = Convert.ToBase64String(salt);
        return saltB64 + "." + hashB64;
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(providedPassword))
        {
            return false;
        }

        var parts = hashedPassword.Split('.', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        var saltB64 = parts[0];
        var storedHashB64 = parts[1];

        byte[] salt;
        byte[] storedHashBytes;

        try
        {
            salt = Convert.FromBase64String(saltB64);
            storedHashBytes = Convert.FromBase64String(storedHashB64);
        }
        catch (FormatException)
        {
            return false;
        }

        var computedHashB64 = HashingHelper.HashPassword(providedPassword, salt);
        byte[] computedHashBytes;

        try
        {
            computedHashBytes = Convert.FromBase64String(computedHashB64);
        }
        catch (FormatException)
        {
            return false;
        }

        if (storedHashBytes.Length != computedHashBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
    }
}