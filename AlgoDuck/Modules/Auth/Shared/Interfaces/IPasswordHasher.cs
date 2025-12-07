namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}