using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.UserModule.Models;

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
        public string SolutionUrl { get; set; }  

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public Problem Problem { get; set; }

        [ForeignKey("Status")]
        public Guid StatusId { get; set; }
        public Status Status { get; set; }

        [ForeignKey("Language")]
        public Guid LanguageId { get; set; }
        public Language Language { get; set; }
    }
}