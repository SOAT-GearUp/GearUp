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
            ?? "Server=localhost,14333;Database=GearUp;User Id=sa;Password=Your_strong!Pass123;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<GearUpDbContext>()
            .UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;

        return new GearUpDbContext(options);
    }
}
