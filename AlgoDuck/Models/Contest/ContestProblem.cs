using System.ComponentModel.DataAnnotations.Schema;
using ProblemModel = AlgoDuck.Modules.Problem.Models.Problem;

namespace AlgoDuck.Modules.Contest.Models
{
    public class ContestProblem
    {
        [ForeignKey("Contest")]
        public Guid ContestId { get; set; }
        public required Contest Contest { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public required ProblemModel Problem { get; set; }
    }
}