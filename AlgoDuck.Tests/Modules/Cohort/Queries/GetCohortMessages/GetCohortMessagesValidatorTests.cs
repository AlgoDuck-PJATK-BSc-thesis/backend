using AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Queries.GetCohortMessages;

public sealed class GetCohortMessagesValidatorTests
{
    private readonly GetCohortMessagesValidator _validator = new();

    [Fact]
    public void Validate_WhenCohortIdEmpty_ThenHasValidationError()
    {
        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = Guid.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenCohortIdValid_ThenIsValid()
    {
        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}