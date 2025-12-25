using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UpdateChatNameController(
    IUpdateChatNameService updateChatNameService
    ) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> UpdateChatName([FromBody] UpdateChatNameDto updateChatNameDto, CancellationToken cancellationToken)
    {
        
        var res = await updateChatNameService.UpdateChatName(updateChatNameDto, cancellationToken);
        return res.Match(
            ok => Ok(new StandardApiResponse<UpdateChatNameResult>
            {
                Body = ok
            }),
            err => Ok(new StandardApiResponse
            {
                Status = Status.Error,
                Message = err
            }));
    }
}

public class UpdateChatNameDto{
    public required string ChatName { get; set; }
    public required string NewChatName { get; set; }
    public required Guid ProblemId { get; set; }
    internal Guid UserId { get; set; }
}

public class UpdateChatNameResult
{
    public required string NewChatName { get; set; }
    public required int MessagesUpdated { get; set; }
}