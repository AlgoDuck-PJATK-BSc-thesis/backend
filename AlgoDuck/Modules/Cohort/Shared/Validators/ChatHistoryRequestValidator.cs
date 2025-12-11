using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Shared.Validators;

public sealed class ChatHistoryRequestValidator<T> : AbstractValidator<T> where T : IChatHistoryRequest
{
    public ChatHistoryRequestValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(ChatConstants.MaxPageSize)
            .When(x => x.PageSize.HasValue);
    }
}

public interface IChatHistoryRequest
{
    Guid CohortId { get; }
    int? PageSize { get; }
}