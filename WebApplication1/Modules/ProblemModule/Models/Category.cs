using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string CategoryName { get; set; }
    }
}