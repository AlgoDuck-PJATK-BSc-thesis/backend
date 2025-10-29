using System.ComponentModel.DataAnnotations.Schema;

namespace AlgoDuck.Models.Problem
{
    public class HasTag
    {
        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public required Problem Problem { get; set; }

        [ForeignKey("Tag")]
        public Guid TagId { get; set; }
        public required Tag Tag { get; set; }
    }
}