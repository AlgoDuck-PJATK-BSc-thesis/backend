using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Session
{
    public Guid SessionId { get; set; } = Guid.NewGuid();

    public string RefreshTokenHash { get; set; } = null!;

    public string RefreshTokenSalt { get; set; } = null!;
    
    public string? RefreshTokenPrefix { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public Guid? ReplacedBySessionId { get; set; }

    public Guid UserId { get; set; }

    public virtual ICollection<Session> InverseReplacedBySession { get; set; } = new List<Session>();

    public virtual Session? ReplacedBySession { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
