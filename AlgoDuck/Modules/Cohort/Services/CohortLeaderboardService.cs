using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Modules.Cohort.Services;

public class CohortLeaderboardService : ICohortLeaderboardService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CohortLeaderboardService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<CohortLeaderboardDto>> GetLeaderboardAsync(Guid cohortId)
    {
        var userId = GetCurrentUserId();

        var belongsToCohort = await _dbContext.ApplicationUsers
            .AnyAsync(u => u.Id == userId && u.CohortId == cohortId);

        if (!belongsToCohort)
            throw new ForbiddenException("You are not a member of this cohort.");

        var users = await _dbContext.ApplicationUsers
            .Where(u => u.CohortId == cohortId)
            .OrderByDescending(u => u.Experience)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Experience
            })
            .ToListAsync();

        var leaderboard = new List<CohortLeaderboardDto>();
        int rank = 1;
        int position = 1;
        int? previousExp = null;

        foreach (var user in users)
        {
            if (previousExp != user.Experience)
            {
                rank = position;
            }

            leaderboard.Add(new CohortLeaderboardDto
            {
                UserId = user.Id,
                Username = user.UserName!,
                Experience = user.Experience,
                Rank = rank
            });

            previousExp = user.Experience;
            position++;
        }

        return leaderboard;
    }

    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
            throw new UnauthorizedException();

        return guid;
    }
}