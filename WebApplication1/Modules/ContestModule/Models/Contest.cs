using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.ItemModule.Models;

namespace WebApplication1.Modules.ContestModule.Models
{
    public class Contest
    {
        [Key]
        public Guid ContestId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string ContestName { get; set; }

        [Required]
        public required string ContestDescription { get; set; }

        [Required]
        public DateTime ContestStartDate { get; set; }

        [Required]
        public DateTime ContestEndDate { get; set; }

        [ForeignKey("ItemId")]
        public Guid ItemId { get; set; }
        public required Item Item { get; set; }
        
        public ICollection<ContestProblem> ContestProblems { get; set; } = new List<ContestProblem>();

    }
}