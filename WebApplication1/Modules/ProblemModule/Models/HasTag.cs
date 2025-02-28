using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Modules.ProblemModule.Models
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