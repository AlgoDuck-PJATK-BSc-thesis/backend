
namespace AlgoDuck.Models;

public partial class Contest
{
    public Guid ContestId { get; set; }

    public string ContestName { get; set; } = null!;

    public string ContestDescription { get; set; } = null!;

    public DateTime ContestStartDate { get; set; }

    public DateTime ContestEndDate { get; set; }

    public Guid ItemId { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
