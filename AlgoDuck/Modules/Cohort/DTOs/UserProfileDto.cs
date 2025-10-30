namespace AlgoDuck.Modules.Cohort.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public int Experience { get; set; }
}