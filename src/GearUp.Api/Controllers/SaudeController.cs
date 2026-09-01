using GearUp.Api.Contracts.Saude;
using GearUp.Api.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GearUp.Api.Controllers;

[ApiController]
[Route("health")]
[Tags("Saúde")]
public sealed class SaudeController(HealthCheckService healthCheckService) : ControllerBase
{
    [AllowAnonymous, HttpGet("live")]
    [ProducesResponseType<SaudeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<SaudeResponse>(StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> Live(CancellationToken ct) =>
        VerificarAsync("live", ct);

    [AllowAnonymous, HttpGet("ready")]
    [ProducesResponseType<SaudeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<SaudeResponse>(StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> Ready(CancellationToken ct) =>
        VerificarAsync("ready", ct);

    private async Task<IActionResult> VerificarAsync(string tag, CancellationToken ct)
    {
        var relatorio = await healthCheckService.CheckHealthAsync(
            registro => registro.Tags.Contains(tag), ct);

        return StatusCode(
            RespostaSaudeFactory.ObterStatusHttp(relatorio.Status),
            RespostaSaudeFactory.Criar(relatorio));
    }
}
