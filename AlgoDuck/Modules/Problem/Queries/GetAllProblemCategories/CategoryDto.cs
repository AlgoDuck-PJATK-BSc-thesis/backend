namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

public class CategoryDto
{
    public required Guid CategoryId { get; set; }
    public required string CategoryName { get; set; }
}