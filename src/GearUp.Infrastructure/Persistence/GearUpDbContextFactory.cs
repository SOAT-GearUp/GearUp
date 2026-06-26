using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GearUp.Infrastructure.Persistence;

internal sealed class GearUpDbContextFactory
    : IDesignTimeDbContextFactory<GearUpDbContext>
{
    // utilizado pelo comando `dotnet ef migrations add` para criar migrações em tempo de design.
    public GearUpDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__GearUpDatabase")
            ?? "Host=localhost;Port=5433;Database=GearUp;Username=gearup;Password=GearUp_Strong!Pass123";

        var options = new DbContextOptionsBuilder<GearUpDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure())
            .Options;

        return new GearUpDbContext(options);
    }
}
