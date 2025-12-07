using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserById;

[ApiController]
[Route("api/user/{userId:guid}")]
[Authorize]
public sealed class GetUserByIdEndpoint : ControllerBase
{
    private readonly IGetUserByIdHandler _handler;
    private readonly GetUserByIdValidator _validator;

    public GetUserByIdEndpoint(
        IGetUserByIdHandler handler,
        GetUserByIdValidator validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdRequestDto { UserId = userId };

        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new StandardApiResponse
            {
                Status = Status.Error,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            });
        }

        try
        {
            var user = await _handler.HandleAsync(query, cancellationToken);

            return Ok(new StandardApiResponse<UserDto>
            {
                Body = user
            });
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = ex.Message
            });
        }
    }
}