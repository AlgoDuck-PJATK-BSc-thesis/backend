using System.ComponentModel.DataAnnotations;
using AlgoDuck.Models.User;

namespace AlgoDuck.Models.Auth;

public class Session
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    
    [MaxLength(512)]
    public required string RefreshTokenHash { get; set; }
    
    [MaxLength(512)]
    public required string RefreshTokenSalt { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    
    public DateTime? RevokedAtUtc { get; set; }
    
    public Guid? ReplacedBySessionId { get; set; }
    public Session? ReplacedBySession { get; set; }
    
    public Guid UserId { get; set; }
    public required ApplicationUser User { get; set; }
    
    public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
    
}

