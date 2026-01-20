using System.Security.Cryptography;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class PasswordGenerator
{
    private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";

    public static string Generate(int length)
    {
        if (length < ValidationRules.PasswordMinLength)
        {
            length = ValidationRules.PasswordMinLength;
        }

        if (length > ValidationRules.PasswordMaxLength)
        {
            length = ValidationRules.PasswordMaxLength;
        }

        var chars = new char[length];
        var bytes = new byte[length];

        RandomNumberGenerator.Fill(bytes);

        for (var i = 0; i < length; i++)
        {
            var index = bytes[i] % AllowedCharacters.Length;
            chars[i] = AllowedCharacters[index];
        }

        return new string(chars);
    }
}