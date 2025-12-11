using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;

public sealed class GetCohortMessagesValidator : AbstractValidator<GetCohortMessagesRequestDto>
{
    public GetCohortMessagesValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(ChatConstants.MaxPageSize)
            .When(x => x.PageSize.HasValue);
    }
}