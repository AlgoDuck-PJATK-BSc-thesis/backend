using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohortNamespace = WebApplication1.Modules.CohortModule.Models;
using WebApplication1.Modules.AuthModule.Models;
using DuelNamespace = WebApplication1.Modules.DuelModule.Models;
using ItemNamespace = WebApplication1.Modules.ItemModule.Models;
using ProblemNamespace = WebApplication1.Modules.ProblemModule.Models;

namespace WebApplication1.Modules.UserModule.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string Username { get; set; }

        [Required, MaxLength(256)]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        [Required]
        public required string Salt { get; set; }

        public int Coins { get; set; }
        public int Experience { get; set; }
        public int AmountSolved { get; set; }

        [MaxLength(256)]
        public required string ProfilePicture { get; set; }

        [ForeignKey("UserRole")]
        public Guid UserRoleId { get; set; }
        public required UserRole UserRole { get; set; }

        [ForeignKey("Cohort")]
        public Guid? CohortId { get; set; }
        public required CohortNamespace.Cohort Cohort { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        
        public ICollection<DuelNamespace.DuelParticipant> DuelParticipants { get; set; } = new List<DuelNamespace.DuelParticipant>();
        public ICollection<ItemNamespace.Purchase> Purchases { get; set; } = new List<ItemNamespace.Purchase>();
        public ICollection<ProblemNamespace.PersonalizedProblem> PersonalizedProblems { get; set; } = new List<ProblemNamespace.PersonalizedProblem>();
        public ICollection<ProblemNamespace.UserSolution> UserSolutions { get; set; } = new List<ProblemNamespace.UserSolution>();

    }
}