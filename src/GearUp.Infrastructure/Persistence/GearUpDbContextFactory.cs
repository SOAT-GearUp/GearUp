using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GearUp.Infrastructure.Persistence;

internal sealed class GearUpDbContextFactory
    : IDesignTimeDbContextFactory<GearUpDbContext>
{
    public GearUpDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__GearUpDatabase")
            ?? "Server=tcp:localhost,14333;Database=GearUp;User Id=sa;Password=Your_strong!Pass123;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<GearUpDbContext>()
            .UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;

        return new GearUpDbContext(options);
    }
}
