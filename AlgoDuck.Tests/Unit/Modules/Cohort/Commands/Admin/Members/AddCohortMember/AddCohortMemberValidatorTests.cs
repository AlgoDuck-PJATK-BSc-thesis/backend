using AlgoDuck.Modules.Cohort.Commands.Admin.Members.AddCohortMember;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Members.AddCohortMember;

public sealed class AddCohortMemberValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new AddCohortMemberValidator();

        var result = v.Validate(new AddCohortMemberDto
        {
            UserId = Guid.NewGuid()
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUserIdEmpty_IsInvalid()
    {
        var v = new AddCohortMemberValidator();

        var result = v.Validate(new AddCohortMemberDto
        {
            UserId = Guid.Empty
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }
}