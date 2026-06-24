using GearUp.Api.Contracts.Clientes.Atualizar;
using GearUp.Api.Contracts.Clientes.Cadastrar;
using GearUp.Api.Contracts.Clientes.Veiculos.Atualizar;
using GearUp.Api.Contracts.Clientes.Veiculos.Cadastrar;
using GearUp.Application.Atendimento.Clientes.Atualizar;
using GearUp.Application.Atendimento.Clientes.Cadastrar;
using GearUp.Application.Atendimento.Clientes.Consultar;
using GearUp.Application.Atendimento.Clientes.Excluir;
using GearUp.Application.Atendimento.Clientes.Listar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers;

[ApiController, Route("api/clientes")]
public sealed class ClientesController(
    ICadastrarClienteUseCase cadastrarClienteUseCase,
    IConsultarClienteUseCase consultarClienteUseCase,
    IListarClienteUseCase listarClienteUseCase,
    IAtualizarClienteUseCase atualizarClienteUseCase,
    IExcluirClienteUseCase excluirClienteUseCase) : ControllerBase
{
    [Authorize(Roles = "Atendente"), HttpGet]
    [ProducesResponseType<List<ListarClienteResult>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var clientes = await listarClienteUseCase.ListarAsync(ct);
        return Ok(clientes);
    }

    [Authorize(Roles = "Atendente"), HttpGet("{id:guid}")]
    [ProducesResponseType<ConsultarClienteResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var cliente = await consultarClienteUseCase.ObterAsync(id, ct);
        return Ok(cliente);
    }

    [Authorize(Roles = "Atendente"), HttpPost]
    [ProducesResponseType<CadastrarClienteResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(CadastrarClienteRequest request, CancellationToken ct)
    {
        var result = await cadastrarClienteUseCase.CadastrarAsync(
            new CadastrarClienteCommand(request.Nome, request.Documento, request.Email, request.Telefone), ct);

        return Created($"/api/clientes/{result.Id}", result);
    }

    [Authorize(Roles = "Atendente"), HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarClienteRequest request, CancellationToken ct)
    {
        await atualizarClienteUseCase.AtualizarAsync(
            new AtualizarClienteCommand(id, request.Nome, request.Email, request.Telefone), ct);

        return NoContent();
    }

    [Authorize(Roles = "Atendente"), HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await excluirClienteUseCase.ExcluirAsync(id, ct);
        return NoContent();
    }
}
