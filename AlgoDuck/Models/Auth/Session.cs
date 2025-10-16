using AlgoDuck.Modules.User.Models;

namespace AlgoDuck.Modules.Auth.Models;

public class Session
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public required string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public bool Revoked { get; set; }

    public Guid UserId { get; set; }
    public required ApplicationUser User { get; set; }
}

