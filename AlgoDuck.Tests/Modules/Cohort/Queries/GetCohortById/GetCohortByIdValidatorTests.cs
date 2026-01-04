using AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetCohortById;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Queries.GetCohortById;

public sealed class GetCohortByIdValidatorTests
{
    private readonly GetCohortByIdValidator _validator = new();

    [Fact]
    public void Validate_WhenCohortIdEmpty_ThenHasValidationError()
    {
        var dto = new GetCohortByIdRequestDto
        {
            CohortId = Guid.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenCohortIdValid_ThenIsValid()
    {
        var dto = new GetCohortByIdRequestDto
        {
            CohortId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}