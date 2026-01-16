namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;

public class CustomLayoutDetailsResponseDto
{
    public required Guid LayoutId { get; set; }
    public required string LayoutName { get; set; }
    public required object LayoutContent { get; set; }
}