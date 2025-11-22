namespace AlgoDuck.Models;

public class TestingResult
{
    public Guid UserSolutionId { get; set; }
    public UserSolution? UserSolution { get; set; }
    public Guid ExerciseId { get; set; }
    public Problem? Problem { get; set; }
    public bool IsPassed { get; set; }

}