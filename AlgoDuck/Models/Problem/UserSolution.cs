using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Models.User.ApplicationUser;

namespace AlgoDuck.Models.Problem
{
    public class UserSolution
    {
        [Key]
        public Guid SolutionId { get; set; } = Guid.NewGuid();

        public int Stars { get; set; } 

        [Required]
        public DateTime CodeRuntimeSubmitted { get; set; } = DateTime.UtcNow;

        [Required]
        public required string SolutionUrl { get; set; }  

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required ApplicationUser User { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public required Problem Problem { get; set; }

        [ForeignKey("Status")]
        public Guid StatusId { get; set; }
        public required Status Status { get; set; }

        [ForeignKey("Language")]
        public Guid LanguageId { get; set; }
        public required Language Language { get; set; }
    }
}