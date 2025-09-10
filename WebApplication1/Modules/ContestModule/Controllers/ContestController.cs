using Microsoft.AspNetCore.Mvc;
using WebApplication1.Modules.Contest.DTOs;
using WebApplication1.Modules.Contest.Services;

namespace WebApplication1.Modules.Contest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContestController : ControllerBase
{
    private readonly IContestService _contestService;

    public ContestController(IContestService contestService)
    {
        _contestService = contestService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateContest([FromBody] CreateContestDto dto)
    {
        var contestId = await _contestService.CreateContestAsync(dto);
        return CreatedAtAction(nameof(GetContestById), new { id = contestId }, contestId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContestById(Guid id)
    {
        var contest = await _contestService.GetContestByIdAsync(id);
        if (contest == null)
        {
            return NotFound();
        }

        return Ok(contest);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteContest(Guid id)
    {
        var result = await _contestService.DeleteContestAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{contestId:guid}/problems/{problemId:guid}")]
    public async Task<IActionResult> AddProblemToContest(Guid contestId, Guid problemId)
    {
        try
        {
            await _contestService.AddProblemToContest(contestId, problemId);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{contestId:guid}/problems/{problemId:guid}")]
    public async Task<IActionResult> RemoveProblemFromContest(Guid contestId, Guid problemId)
    {
        try
        {
            await _contestService.RemoveProblemFromContest(contestId, problemId);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}