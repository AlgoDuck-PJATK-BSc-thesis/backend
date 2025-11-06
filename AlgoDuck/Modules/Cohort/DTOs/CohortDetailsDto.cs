namespace AlgoDuck.Modules.Cohort.DTOs;

public class CohortDetailsDto
{
    public Guid CohortId { get; set; }
    public string Name { get; set; } = default!;
    public CohortCreatorDto CreatedBy { get; set; } = default!;
    public int MemberCount { get; set; }
    public List<UserProfileDto> Members { get; set; } = new();
}