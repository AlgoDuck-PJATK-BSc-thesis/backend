using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.AddCohortMember;

public sealed class AdminAddCohortMemberValidator : AbstractValidator<AdminAddCohortMemberDto>
{
    public AdminAddCohortMemberValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}