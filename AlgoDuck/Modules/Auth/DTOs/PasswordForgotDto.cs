namespace AlgoDuck.Modules.Auth.DTOs
{
    public sealed class PasswordForgotDto
    {
        public string Email { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}