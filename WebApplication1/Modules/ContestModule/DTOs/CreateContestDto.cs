namespace WebApplication1.Modules.Contest.DTOs;

public class CreateContestDto
{
    public string ContestName { get; set; } = string.Empty;
    public string ContestDescription { get; set; } = string.Empty;
    public DateTime ContestStartDateTime { get; set; }
    public DateTime ContestEndDateTime { get; set; }
    public Guid ItemId { get; set; }
    public List<Guid> ProblemIds { get; set; } = new();
}