using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GearUp.Infrastructure.Persistence;

internal sealed class DatabaseHealthCheck(GearUpDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return await db.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Não foi possível conectar ao PostgreSQL.");
    }
}
