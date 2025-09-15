using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Modules.User.Models.ApplicationUser;
using UserNamespace = AlgoDuck.Modules.User.Models;

namespace AlgoDuck.Modules.Problem.Models
{
    public class PersonalizedProblem
    {
        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public required Problem Problem { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required ApplicationUser User { get; set; }
    }
}