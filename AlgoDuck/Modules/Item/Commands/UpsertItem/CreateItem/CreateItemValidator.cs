using AlgoDuck.Modules.Item.Commands.UpsertItem.UpdateItem;
using FluentValidation;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;

public class CreateItemValidator : AbstractValidator<ItemUpdateRequestDto>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Description).MaximumLength(1024).WithMessage("Description cannot exceed 1024 characters");
        RuleFor(x => x.ItemCost).NotEmpty().GreaterThan(0).WithMessage("ItemCost should be greater than 0");
        RuleFor(x => x.ItemName).NotEmpty().WithMessage("Item name cannot be empty").MaximumLength(256).WithMessage("Item name cannot exceed 256 characters");
    }
}