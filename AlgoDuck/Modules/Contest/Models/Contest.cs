using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ItemModel = AlgoDuck.Modules.Item.Models.Item;

namespace AlgoDuck.Modules.Contest.Models
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
        public required ItemModel Item { get; set; }
        
        public ICollection<ContestProblem> ContestProblems { get; set; } = new List<ContestProblem>();

    }
}