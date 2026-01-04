using AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortActiveMembers;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Queries.GetCohortActiveMembers;

public sealed class GetCohortActiveMembersValidatorTests
{
    private readonly GetCohortActiveMembersValidator _validator = new();

    [Fact]
    public void Validate_WhenCohortIdEmpty_ThenHasValidationError()
    {
        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = Guid.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenCohortIdValid_ThenIsValid()
    {
        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}