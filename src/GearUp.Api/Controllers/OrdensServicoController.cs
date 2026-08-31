using GearUp.Api.Authorization;
using GearUp.Api.Contracts.OrdemServico;
using GearUp.Application.OrdemDeServico.Ordens.Consultar;
using GearUp.Application.OrdemDeServico.Ordens.ConsultarStatus;
using GearUp.Application.OrdemDeServico.Ordens.Criar;
using GearUp.Application.OrdemDeServico.Ordens.Listar;
using GearUp.Application.OrdemDeServico.Orcamentos.Criar;
using GearUp.Application.OrdemDeServico.Diagnosticos.IniciarDiagnostico;
using GearUp.Application.OrdemDeServico.Diagnosticos.RegistrarDiagnostico;
using GearUp.Application.OrdemDeServico.Execucao.AlterarStatus;
using GearUp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearUp.Api.Controllers;

[ApiController, Route("api/ordens-servico"), Authorize]
public sealed class OrdensServicoController(
    ICriarOrdemServicoUseCase criarOrdemServicoUseCase,
    IListarOrdemServicoUseCase listarOrdemServicoUseCase,
    IConsultarOrdemServicoUseCase consultarOrdemServicoUseCase,
    IConsultarStatusOrdemServicoUseCase consultarStatusOrdemServicoUseCase,
    IIniciarDiagnosticoUseCase iniciarDiagnosticoUseCase,
    IRegistrarDiagnosticoUseCase registrarDiagnosticoUseCase,
    IAlterarStatusUseCase alterarStatusUseCase) : ControllerBase
{
    // Passar lista de Serviços e Peças no request de criação da OS, para que o sistema já calcule o valor total da OS.
    [HttpPost, Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar(CriarOrdemServicoRequest request, CancellationToken ct)
    {
        var itens = request.Itens?
            .Select(i => new CriarItemOrcamentoCommand(i.Tipo, i.Descricao, i.Quantidade, i.ValorUnitario, i.EstoqueItemId))
            .ToList();

        var os = await criarOrdemServicoUseCase.CriarAsync(
            new CriarOrdemServicoCommand(
                request.ClienteId,
                request.VeiculoId,
                request.SolicitacaoInicial,
                request.Prioridade,
                request.Prazo,
                itens
            ), ct);

        return Created($"/api/ordens-servico/{os.Id}", os);
    }

    [HttpGet]
    [ProducesResponseType<List<ListarOrdemServicoResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar([FromQuery] bool emAndamento, [FromQuery] Guid? clienteId, CancellationToken ct)
    {
        if (User.IsInRole("Cliente"))
            clienteId = ObterClienteId();

        var ordens = await listarOrdemServicoUseCase.ListarAsync(
            new ListarOrdemServicoCommand(emAndamento, clienteId), ct);

        return Ok(ordens);
    }

    [HttpGet("{ordemServicoId:guid}")]
    [ProducesResponseType<ConsultarOrdemServicoResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(Guid ordemServicoId, CancellationToken ct)
    {
        var os = await consultarOrdemServicoUseCase.ObterAsync(new ConsultarOrdemServicoCommand(ordemServicoId), ct);

        if (!User.PodeAcessarOrdemServico(os.ClienteId))
            return NotFound(new { code = "OS_NAO_ENCONTRADA", message = "Ordem de serviço não encontrada." });

        return Ok(os);
    }

    [HttpGet("{ordemServicoId:guid}/status")]
    [ProducesResponseType<ConsultarStatusOrdemServicoResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterStatus(Guid ordemServicoId, CancellationToken ct)
    {
        var status = await consultarStatusOrdemServicoUseCase.ObterAsync(new ConsultarStatusOrdemServicoCommand(ordemServicoId), ct);

        if (!User.PodeAcessarOrdemServico(status.ClienteId))
            return NotFound(new { code = "OS_NAO_ENCONTRADA", message = "Ordem de serviço não encontrada." });

        return Ok(status);
    }

    [HttpPost("{ordemServicoId:guid}/diagnostico/iniciar"), Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> IniciarDiagnostico(Guid ordemServicoId, CancellationToken ct)
    {
        await iniciarDiagnosticoUseCase.IniciarAsync(
            new IniciarDiagnosticoCommand(
                ordemServicoId,
                Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!)
            ), ct);

        return NoContent();
    }

    [HttpPost("{ordemServicoId:guid}/diagnostico"), Authorize(Roles = "Admin,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Diagnosticar(Guid ordemServicoId, DiagnosticoRequest request, CancellationToken ct)
    {
        await registrarDiagnosticoUseCase.RegistrarAsync(
            new RegistrarDiagnosticoCommand(ordemServicoId, request.Descricao), ct);

        return NoContent();
    }

    [HttpPatch("{ordemServicoId:guid}/status"), Authorize(Roles = "Admin,Atendente,Auxiliar,Mecanico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AlterarStatus(Guid ordemServicoId, AlterarStatusRequest request, CancellationToken ct)
    {
        await alterarStatusUseCase.AlterarAsync(
            new AlterarStatusCommand(ordemServicoId, request.Status), ct);

        return NoContent();
    }

    private Guid ObterClienteId()
    {
        return Guid.TryParse(User.FindFirstValue("cliente_id"), out var id)
            ? id
            : throw new UnauthorizedAccessException("Usuário não vinculado a um cliente.");
    }
}
