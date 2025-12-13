using AlgoDuck.Modules.Cohort.Shared.Validators;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Validators;

public sealed class ChatHistoryRequestValidatorTests
{
    private sealed class TestRequest : IChatHistoryRequest
    {
        public Guid CohortId { get; init; }
        public int? PageSize { get; init; }
    }

    private readonly ChatHistoryRequestValidator<TestRequest> _validator = new();

    [Fact]
    public void Validate_WhenCohortIdIsEmpty_ThenHasValidationError()
    {
        var request = new TestRequest
        {
            CohortId = Guid.Empty,
            PageSize = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenCohortIdIsNotEmpty_ThenHasNoValidationErrorForCohortId()
    {
        var request = new TestRequest
        {
            CohortId = Guid.NewGuid(),
            PageSize = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenPageSizeIsNull_ThenHasNoValidationErrorForPageSize()
    {
        var request = new TestRequest
        {
            CohortId = Guid.NewGuid(),
            PageSize = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeIsZero_ThenHasValidationError()
    {
        var request = new TestRequest
        {
            CohortId = Guid.NewGuid(),
            PageSize = 0
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeIsNegative_ThenHasValidationError()
    {
        var request = new TestRequest
        {
            CohortId = Guid.NewGuid(),
            PageSize = -5
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeIsWithinAllowedRange_ThenHasNoValidationError()
    {
        var request = new TestRequest
        {
            CohortId = Guid.NewGuid(),
            PageSize = 10
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
}