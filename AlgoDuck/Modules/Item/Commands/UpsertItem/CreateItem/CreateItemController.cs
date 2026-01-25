using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem.Types;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;

[ApiController]
[Route("/api/admin/item")]
[Authorize(Roles = "admin")]
public class CreateItemController : ControllerBase
{
    private readonly ICreateItemService _createItemService;
    private readonly IValidator<CreateItemRequestDto> _validator;

    public CreateItemController(ICreateItemService createItemService, IValidator<CreateItemRequestDto> validator)
    {
        _createItemService = createItemService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateItemAsync(
        [FromForm] CreateItemRequestDto createItemDto, 
        CancellationToken cancellation)
    {
        var validationResult = await _validator.ValidateAsync(createItemDto, cancellation);
        if (!validationResult.IsValid)
            return new ObjectResult(new StandardApiResponse<IEnumerable<ValidationFailure>>
            {
                Status = Status.Error,
                Body = validationResult.Errors
            }){ StatusCode = 422 };
        
        return await User
            .GetUserId()
            .BindAsync(async user =>
            {
                createItemDto.CreatedByUserId = user;
                return await _createItemService.CreateItemAsync(createItemDto, cancellation);
            }).ToActionResultAsync();
    }    
}
