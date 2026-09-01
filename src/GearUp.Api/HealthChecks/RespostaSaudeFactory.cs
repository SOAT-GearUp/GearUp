using GearUp.Api.Contracts.Saude;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GearUp.Api.HealthChecks;

public static class RespostaSaudeFactory
{
    public static SaudeResponse Criar(HealthReport relatorio)
    {
        var verificacoes = relatorio.Entries
            .Select(entrada => new VerificacaoSaudeResponse(
                entrada.Key,
                entrada.Value.Status.ToString(),
                entrada.Value.Description))
            .ToArray();

        return new SaudeResponse(
            relatorio.Status.ToString(),
            VersaoAplicacao.Atual,
            relatorio.TotalDuration.TotalMilliseconds,
            verificacoes);
    }

    // As probes do Kubernetes só consideram o status HTTP: 200 para saudável ou
    // degradado, 503 para indisponível.
    public static int ObterStatusHttp(HealthStatus status) =>
        status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;
}
