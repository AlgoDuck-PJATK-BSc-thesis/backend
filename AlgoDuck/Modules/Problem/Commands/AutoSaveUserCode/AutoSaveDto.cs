using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Problem.Shared.Types;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

public class AutoSaveDto
{
    public required Guid ProblemId { get; set; }
    
    public required string UserCodeB64 { get; init; }
    internal Guid UserId { get; set; }
}

public class TestingResultSnapshotUpdate
{
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
    public ICollection<TestResultDto> TestingResults { get; set; } = [];
}
