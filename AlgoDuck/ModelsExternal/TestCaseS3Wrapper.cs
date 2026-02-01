namespace AlgoDuck.ModelsExternal;


public class TestCaseCallArg
{
    public required string VarName { get; set; }
    public bool IsMutated { get; set; } = false;
}

/*
 * Since only some of the test case data is stores in rdb,
 * We need a model to represent the data stored on S3
 */ 
public class TestCaseS3Partial
{
    public Guid TestCaseId { get; set; }
    public string Setup { get; set; } = string.Empty; // Arrange
    public List<TestCaseCallArg> Call { get; set; } = []; // Act
    public string Expected { get; set; } = string.Empty; // Assert
}

public class TestCaseS3WrapperObject
{
    public Guid ProblemId { get; set; }
    public List<TestCaseS3Partial> TestCases { get; set; } = [];
}