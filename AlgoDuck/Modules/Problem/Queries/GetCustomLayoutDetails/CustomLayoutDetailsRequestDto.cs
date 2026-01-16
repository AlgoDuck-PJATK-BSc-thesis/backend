namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;

public class CustomLayoutDetailsRequestDto
{
    public required Guid LayoutId { get; set; }
    internal Guid UserId { get; set; }
}