using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class PersonalizedProblem
    {
        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public Problem Problem { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}