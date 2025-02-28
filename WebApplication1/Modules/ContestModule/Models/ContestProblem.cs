using System.ComponentModel.DataAnnotations.Schema;
using ProblemNamespace = WebApplication1.Modules.ProblemModule.Models;

namespace WebApplication1.Modules.ContestModule.Models
{
    public class ContestProblem
    {
        [ForeignKey("Contest")]
        public Guid ContestId { get; set; }
        public required Contest Contest { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public required ProblemNamespace.Problem Problem { get; set; }
    }
}