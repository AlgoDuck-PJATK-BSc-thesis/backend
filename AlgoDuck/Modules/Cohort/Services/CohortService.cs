using Microsoft.EntityFrameworkCore;
using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Modules.Cohort.Services;

public class CohortService : ICohortService
{
    private readonly ApplicationDbContext _db;

    public CohortService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CohortDto>> GetAllAsync() =>
        await _db.Cohorts.Select(c => new CohortDto
        {
            CohortId = c.CohortId,
            Name = c.Name
        }).ToListAsync();

    public async Task<CohortDto?> GetByIdAsync(Guid id) =>
        await _db.Cohorts.Where(c => c.CohortId == id)
            .Select(c => new CohortDto
            {
                CohortId = c.CohortId,
                Name = c.Name
            })
            .FirstOrDefaultAsync();

    public async Task<CohortDto> CreateAsync(CreateCohortDto dto, Guid currentUserId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null)
            throw new NotFoundException("User not found.");

        var alreadyCreated = await _db.Cohorts.AnyAsync(c => c.CreatedByUserId == currentUserId);
        var alreadyInCohort = user.CohortId != null;

        if (alreadyCreated || alreadyInCohort)
            throw new AlreadyInCohortException();

        var cohort = new Models.Cohort.Cohort
        {
            Name = dto.Name,
            CreatedByUserId = currentUserId
        };

        _db.Cohorts.Add(cohort);
        user.Cohort = cohort;

        await _db.SaveChangesAsync();

        return new CohortDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
        };
    }

    public async Task<CohortDto?> GetMineAsync(Guid currentUserId)
    {
        return await _db.Cohorts
            .Where(c => c.CreatedByUserId == currentUserId)
            .Select(c => new CohortDto
            {
                CohortId = c.CohortId,
                Name = c.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCohortDto dto)
    {
        var cohort = await _db.Cohorts.FindAsync(id);
        if (cohort is null) return false;
        if (dto.Name is not null) cohort.Name = dto.Name;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var cohort = await _db.Cohorts.FindAsync(id);
        if (cohort is null) return false;

        _db.Cohorts.Remove(cohort);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddUserAsync(Guid cohortId, Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;

        user.CohortId = cohortId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserAsync(Guid cohortId, Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null || user.CohortId != cohortId) return false;

        user.CohortId = null;
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> UserBelongsToCohortAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.CohortId == cohortId, cancellationToken);
    }

    public async Task<List<UserProfileDto>> GetUsersAsync(Guid cohortId) =>
        await _db.Users
            .Where(u => u.CohortId == cohortId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Username = u.UserName!,
                Experience = u.Experience
            })
            .ToListAsync();
    
    public async Task<CohortDetailsDto?> GetDetailsAsync(Guid cohortId, Guid requesterId, bool isAdmin, CancellationToken ct)
    {
        var baseInfo = await _db.Cohorts
            .AsNoTracking()
            .Where(c => c.CohortId == cohortId)
            .Select(c => new
            {
                c.CohortId,
                c.Name,
                c.CreatedByUserId,
                CreatorUsername = c.CreatedByUser.UserName!,
                MemberCount = c.Users.Count
            })
            .FirstOrDefaultAsync(ct);

        if (baseInfo is null) return null;

        if (!isAdmin)
        {
            var belongs = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == requesterId && u.CohortId == cohortId, ct);

            if (!belongs)
                throw new ForbiddenException("You are not a member of this cohort.");
        }

        var members = await _db.Users
            .AsNoTracking()
            .Where(u => u.CohortId == cohortId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Username = u.UserName!,
                Experience = u.Experience
            })
            .ToListAsync(ct);

        return new CohortDetailsDto
        {
            CohortId = baseInfo.CohortId,
            Name = baseInfo.Name,
            CreatedBy = new CohortCreatorDto
            {
                UserId = baseInfo.CreatedByUserId,
                Username = baseInfo.CreatorUsername
            },
            MemberCount = baseInfo.MemberCount,
            Members = members
        };
    }
}