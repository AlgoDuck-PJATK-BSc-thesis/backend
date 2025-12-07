using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AlgoDuck.Shared.Utilities
{
    public static class HashingHelper
    {
        private const int SaltSize = 16; 
        private const int HashSize = 32; 
        private const int Iterations = 100000; 
        
        public static string HashPassword(string password, byte[] salt)
        {
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            return Convert.ToBase64String(hash);
        }
        
        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt); 
            return salt;
        }
        
        public static bool VerifyPassword(string password, string storedHash, byte[] storedSalt)
        {
            byte[] hashedPassword = Convert.FromBase64String(HashPassword(password, storedSalt));
            byte[] storedPasswordHash = Convert.FromBase64String(storedHash);

            return CryptographicOperations.FixedTimeEquals(hashedPassword, storedPasswordHash);
        }

        public static string GenerateSaltB64(int size = 16)
        {
            var salt = new byte[size];
            RandomNumberGenerator.Fill(salt);
            return Convert.ToBase64String(salt);
        }

        public static string HashWithSaltB64(string value, string saltB64)
        {
            var combined = System.Text.Encoding.UTF8.GetBytes(value + saltB64);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }
        
        public static bool VerifyWithSaltB64(string value, string saltB64, string expectedHashB64)
        {
            var computed = HashWithSaltB64(value, saltB64);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computed),
                Convert.FromBase64String(expectedHashB64)
            );
        }
    }
}
