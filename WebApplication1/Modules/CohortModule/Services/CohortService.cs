using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.DAL;
using WebApplication1.Modules.CohortModule.DTOs;
using WebApplication1.Modules.CohortModule.Interfaces;
using WebApplication1.Shared.Exceptions;

namespace WebApplication1.Modules.CohortModule.Services;

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
            Name = c.Name,
            ImageUrl = c.ImageUrl
        }).ToListAsync();

    public async Task<CohortDto?> GetByIdAsync(Guid id) =>
        await _db.Cohorts.Where(c => c.CohortId == id)
            .Select(c => new CohortDto
            {
                CohortId = c.CohortId,
                Name = c.Name,
                ImageUrl = c.ImageUrl
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

        var cohort = new Models.Cohort
        {
            Name = dto.Name,
            ImageUrl = dto.ImageUrl,
            CreatedByUserId = currentUserId
        };

        _db.Cohorts.Add(cohort);
        user.Cohort = cohort;

        await _db.SaveChangesAsync();

        return new CohortDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            ImageUrl = cohort.ImageUrl
        };
    }

    public async Task<CohortDto?> GetMineAsync(Guid currentUserId)
    {
        return await _db.Cohorts
            .Where(c => c.CreatedByUserId == currentUserId)
            .Select(c => new CohortDto
            {
                CohortId = c.CohortId,
                Name = c.Name,
                ImageUrl = c.ImageUrl
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCohortDto dto)
    {
        var cohort = await _db.Cohorts.FindAsync(id);
        if (cohort is null) return false;

        if (dto.Name is not null) cohort.Name = dto.Name;
        if (dto.ImageUrl is not null) cohort.ImageUrl = dto.ImageUrl;

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

    public async Task<List<UserProfileDto>> GetUsersAsync(Guid cohortId) =>
        await _db.Users
            .Where(u => u.CohortId == cohortId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Username = u.UserName!,
                ProfilePicture = u.ProfilePicture,
                Experience = u.Experience
            })
            .ToListAsync();
}