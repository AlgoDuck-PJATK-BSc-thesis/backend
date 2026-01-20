using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.DAL;

public sealed partial class ApplicationCommandDbContext(DbContextOptions<ApplicationCommandDbContext> options)
    : BaseDbContext(options);