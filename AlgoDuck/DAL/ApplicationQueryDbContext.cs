using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.DAL;

public sealed partial class ApplicationQueryDbContext : BaseDbContext
{
    public ApplicationQueryDbContext(DbContextOptions<ApplicationQueryDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
}