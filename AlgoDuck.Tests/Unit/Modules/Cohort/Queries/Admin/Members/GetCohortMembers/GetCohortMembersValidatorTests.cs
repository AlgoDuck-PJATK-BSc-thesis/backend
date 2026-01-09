using AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

public sealed class GetCohortMembersValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new GetCohortMembersValidator();

        var result = v.Validate(new GetCohortMembersRequestDto
        {
            CohortId = Guid.NewGuid()
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCohortIdEmpty_IsInvalid()
    {
        var v = new GetCohortMembersValidator();

        var result = v.Validate(new GetCohortMembersRequestDto
        {
            CohortId = Guid.Empty
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CohortId");
    }
}