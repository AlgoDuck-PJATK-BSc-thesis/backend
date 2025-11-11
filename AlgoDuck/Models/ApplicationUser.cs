using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Models;

public partial class ApplicationUser : IdentityUser<Guid>
{
    public Guid UserId { get; set; }

    public int Coins { get; set; }

    public int Experience { get; set; }

    public int AmountSolved { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string SecurityStamp { get; set; } = null!;

    public Guid? CohortId { get; set; }

    public virtual Cohort? Cohort { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual UserConfig? UserConfig { get; set; }

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();
}
