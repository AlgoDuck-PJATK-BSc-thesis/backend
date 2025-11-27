using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlgoDuck.DAL;

public class ApplicationCommandDbContextFactory : IDesignTimeDbContextFactory<ApplicationCommandDbContext>
{
    public ApplicationCommandDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationCommandDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=application;Username=application;Password=asd123");

        return new ApplicationCommandDbContext(optionsBuilder.Options);
    }
}