using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DuelNamespace = WebApplication1.Modules.DuelModule.Models;
using ContestNamespace = WebApplication1.Modules.ContestModule.Models;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Problem
    {
        [Key]
        public Guid ProblemId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string ProblemTitle { get; set; }

        [Required]
        public required string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public required Category Category { get; set; }

        [ForeignKey("Difficulty")]
        public Guid DifficultyId { get; set; }
        public required Difficulty Difficulty { get; set; }

        [ForeignKey("ProblemType")]
        public Guid ProblemTypeId { get; set; }
        public required ProblemType ProblemType { get; set; }

        [ForeignKey("Duel")]
        public Guid? DuelId { get; set; }
        public required DuelNamespace.Duel Duel { get; set; }

        public ICollection<ProblemTemplate> ProblemTemplates { get; set; } = new List<ProblemTemplate>();
        public ICollection<ContestNamespace.ContestProblem> ContestProblems { get; set; } = new List<ContestNamespace.ContestProblem>();
        public ICollection<HasTag> HasTags { get; set; } = new List<HasTag>();
        public ICollection<PersonalizedProblem> PersonalizedProblems { get; set; } = new List<PersonalizedProblem>();
        public ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();

    }
}

