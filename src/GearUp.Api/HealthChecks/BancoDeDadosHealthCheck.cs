using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GearUp.Api.HealthChecks;

/// <summary>
/// Verifica se a API consegue abrir conexão com o PostgreSQL. Usado apenas pelo
/// endpoint de readiness (/health/ready): o pod sai do balanceamento enquanto o
/// banco está indisponível, sem ser reiniciado.
/// </summary>
public sealed class BancoDeDadosHealthCheck(GearUpDbContext db) : IHealthCheck
{
    public const string Nome = "postgres";
    public const string TagProntidao = "prontidao";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL acessível.")
                : HealthCheckResult.Unhealthy("PostgreSQL inacessível.");
        }
        catch (Exception excecao)
        {
            return HealthCheckResult.Unhealthy("Falha ao consultar o PostgreSQL.", excecao);
        }
    }
}
