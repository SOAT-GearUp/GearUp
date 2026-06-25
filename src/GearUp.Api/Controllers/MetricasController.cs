using GearUp.Application.Execucao.Metricas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers;

[Route("api/ordens-servico/metricas")]
[ApiController]
public class MetricasController(
    IObterTempoMedioExecucaoUseCase obterTempoMedioExecucaoUseCase) : ControllerBase
{
    [HttpGet("tempo-medio-execucao"), Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> TempoMedio(CancellationToken ct)
    {
        var tempo = await obterTempoMedioExecucaoUseCase.ObterTempoMedioExecucaoAsync(ct);

        return Ok(new { tempoMedioSegundos = tempo?.TempoMedio?.TotalSeconds });
    }
}
