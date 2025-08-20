namespace WebApplication1.Modules.CohortModule.DTOs;

public class CohortDto
{
    public Guid CohortId { get; set; }
    public string Name { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
}