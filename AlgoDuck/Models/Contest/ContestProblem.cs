using System.ComponentModel.DataAnnotations.Schema;
using ProblemModel = AlgoDuck.Models.Problem.Problem;

namespace AlgoDuck.Models.Contest
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