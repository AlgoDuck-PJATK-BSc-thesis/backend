using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Members.AddCohortMember;

public sealed class AddCohortMemberValidator : AbstractValidator<AddCohortMemberDto>
{
    public AddCohortMemberValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}