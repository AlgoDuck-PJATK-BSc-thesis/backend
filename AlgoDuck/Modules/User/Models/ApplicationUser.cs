using Microsoft.AspNetCore.Identity;
using CohortModel = AlgoDuck.Modules.Cohort.Models.Cohort;
using UserRoleModel = AlgoDuck.Modules.User.Models.UserRole;
using SessionModel = AlgoDuck.Modules.Auth.Models.Session;
using DuelParticipantModel = AlgoDuck.Modules.Duel.Models.DuelParticipant;
using PurchaseModel = AlgoDuck.Modules.Item.Models.Purchase;
using PersonalizedProblemModel = AlgoDuck.Modules.Problem.Models.PersonalizedProblem;
using UserSolutionModel = AlgoDuck.Modules.Problem.Models.UserSolution;

namespace AlgoDuck.Modules.User.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public int Coins { get; set; } = 0;
    public int Experience { get; set; } = 0;
    public int AmountSolved { get; set; } = 0;
    public string? ProfilePicture { get; set; }

    public Guid? CohortId { get; set; }
    public CohortModel? Cohort { get; set; } 

    public Guid? UserRoleId { get; set; }
    public UserRoleModel? UserRole { get; set; }

    public ICollection<SessionModel>? Sessions { get; set; }
    public ICollection<DuelParticipantModel>? DuelParticipants { get; set; }
    public ICollection<PurchaseModel>? Purchases { get; set; }
    public ICollection<PersonalizedProblemModel>? PersonalizedProblems { get; set; }
    public ICollection<UserSolutionModel>? UserSolutions { get; set; }
}