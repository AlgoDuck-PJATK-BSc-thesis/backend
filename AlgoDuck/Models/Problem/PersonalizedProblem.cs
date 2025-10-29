using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Models.User.ApplicationUser;

namespace AlgoDuck.Models.Problem
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