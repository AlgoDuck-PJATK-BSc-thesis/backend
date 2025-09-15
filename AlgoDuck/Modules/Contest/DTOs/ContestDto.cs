namespace AlgoDuck.Modules.Contest.DTOs;

public class ContestDto
{
    public Guid ContestId { get; set; }
    public string ContestName { get; set; } = string.Empty;
    public string ContestDescription { get; set; } = string.Empty;
    public DateTime ContestStartDateTime { get; set; }
    public DateTime ContestEndDateTime { get; set; }

    public bool isActive => DateTime.UtcNow >= ContestStartDateTime && DateTime.UtcNow <= ContestEndDateTime;
}