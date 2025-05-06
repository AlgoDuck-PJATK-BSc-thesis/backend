using Microsoft.AspNetCore.Identity;
using WebApplication1.Modules.AuthModule.Models;
using WebApplication1.Modules.DuelModule.Models;
using WebApplication1.Modules.ItemModule.Models;
using WebApplication1.Modules.ProblemModule.Models;
using WebApplication1.Modules.CohortModule.Models;

namespace WebApplication1.Modules.UserModule.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public int Coins { get; set; } = 0;
    public int Experience { get; set; } = 0;
    public int AmountSolved { get; set; } = 0;
    public string? ProfilePicture { get; set; }
    public Guid? CohortId { get; set; }
    public CohortModule.Models.Cohort? Cohort { get; set; } 
    public Guid? UserRoleId { get; set; }
    public UserRole? UserRole { get; set; }
    public ICollection<Session>? Sessions { get; set; }
    public ICollection<DuelParticipant>? DuelParticipants { get; set; }
    public ICollection<Purchase>? Purchases { get; set; }
    public ICollection<PersonalizedProblem>? PersonalizedProblems { get; set; }
    public ICollection<UserSolution>? UserSolutions { get; set; }
}