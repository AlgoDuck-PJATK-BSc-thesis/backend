using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.DuelModule.Models;
using ContestNamespace = WebApplication1.Modules.ContestModule.Models;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Problem
    {
        [Key]
        public Guid ProblemId { get; set; } = Guid.NewGuid();
        
        [Required, MaxLength(256)] 
        public string ProblemTitle { get; set; } = string.Empty;

        [Required, MaxLength(1024)]
        public string Description { get; set; }= string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        [ForeignKey("Difficulty")]
        public Guid DifficultyId { get; set; }
        public Difficulty? Difficulty { get; set; }

        [ForeignKey("ProblemType")]
        public Guid ProblemTypeId { get; set; }
        public ProblemType? ProblemType { get; set; }

        [ForeignKey("Duel")]
        public Guid? DuelId { get; set; }
        public Duel? Duel { get; set; }

        public ICollection<ProblemTemplate> ProblemTemplates { get; set; } = new List<ProblemTemplate>();
        public ICollection<ContestNamespace.ContestProblem> ContestProblems { get; set; } = new List<ContestNamespace.ContestProblem>();
        public ICollection<HasTag> HasTags { get; set; } = new List<HasTag>();
        public ICollection<PersonalizedProblem> PersonalizedProblems { get; set; } = new List<PersonalizedProblem>();
        public ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();

    }
}

