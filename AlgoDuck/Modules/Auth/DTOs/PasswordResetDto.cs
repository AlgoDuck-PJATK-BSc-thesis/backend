namespace AlgoDuck.Modules.Auth.DTOs
{
    public sealed class PasswordResetDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}