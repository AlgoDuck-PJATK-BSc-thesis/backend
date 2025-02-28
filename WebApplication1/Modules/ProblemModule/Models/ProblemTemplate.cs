using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class ProblemTemplate
    {
        [Key]
        public Guid ProblemTemplateId { get; set; } = Guid.NewGuid();

        [Required]
        public required string TemplateUrl { get; set; }

        [Required]
        public required string TestCasesUrl { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public required Problem Problem { get; set; }

        [ForeignKey("Language")]
        public Guid LanguageId { get; set; }
        public required Language Language { get; set; }
    }
}