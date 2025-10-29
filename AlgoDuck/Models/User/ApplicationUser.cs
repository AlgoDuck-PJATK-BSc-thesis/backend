using Microsoft.AspNetCore.Identity;
using CohortModel = AlgoDuck.Models.Cohort.Cohort;
using UserRoleModel = AlgoDuck.Models.User.UserRole;
using SessionModel = AlgoDuck.Models.Auth.Session;
using DuelParticipantModel = AlgoDuck.Models.Duel.DuelParticipant;
using PurchaseModel = AlgoDuck.Models.Item.Purchase;
using PersonalizedProblemModel = AlgoDuck.Models.Problem.PersonalizedProblem;
using UserSolutionModel = AlgoDuck.Models.Problem.UserSolution;

namespace AlgoDuck.Models.User;

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