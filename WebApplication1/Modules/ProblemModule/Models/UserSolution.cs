using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserNamespace = WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.ProblemModule.Models
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
        public required UserNamespace.User User { get; set; }

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