using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class ProblemTemplate
    {
        [Key]
        public Guid ProblemTemplateId { get; set; } = Guid.NewGuid();

        [Required]
        public string TemplateUrl { get; set; }

        [Required]
        public string TestCasesUrl { get; set; }

        [ForeignKey("Problem")]
        public Guid ProblemId { get; set; }
        public Problem Problem { get; set; }

        [ForeignKey("Language")]
        public Guid LanguageId { get; set; }
        public Language Language { get; set; }
    }
}