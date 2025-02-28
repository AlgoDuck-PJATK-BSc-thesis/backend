using System;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.ProblemModule.Models;

namespace WebApplication1.Modules.ContestModule.Models
{
    public class ContestProblem
    {
        [ForeignKey("Contest")]
        public Guid ContestId { get; set; }
        public Contest Contest { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public Problem Problem { get; set; }
    }
}