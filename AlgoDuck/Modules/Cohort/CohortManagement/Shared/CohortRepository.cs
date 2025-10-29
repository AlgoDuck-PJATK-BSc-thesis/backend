using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Shared;

public sealed class CohortRepository : ICohortRepository
{
    private readonly ApplicationDbContext _db;
    public CohortRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<CohortDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Cohorts
            .AsNoTracking()
            .Select(c => new CohortDto
            {
                CohortId = c.CohortId,
                Name = c.Name,
                ImageUrl = c.ImageUrl
            })
            .ToListAsync(ct);
    }

    public async Task<Guid> CreateAsync(string name, string imageUrl, Guid createdByUserId, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        _db.Cohorts.Add(new Models.Cohort.Cohort
        {
            CohortId = id,
            Name = name,
            ImageUrl = imageUrl,
            CreatedByUserId = createdByUserId
        });
        await _db.SaveChangesAsync(ct);
        return id;
    }
}