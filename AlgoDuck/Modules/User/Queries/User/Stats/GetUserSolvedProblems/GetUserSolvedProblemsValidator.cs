using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.User.Stats.GetUserSolvedProblems;

public sealed class GetUserSolvedProblemsValidator : AbstractValidator<GetUserSolvedProblemsQuery>
{
    public GetUserSolvedProblemsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}