using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserById;

public sealed class GetUserByIdValidator : AbstractValidator<GetUserByIdRequestDto>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}