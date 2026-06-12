using GearUp.Api.Contracts.Clientes;
using GearUp.Application.Clientes.Cadastrar;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    ICadastrarClienteUseCase cadastrarClienteUseCase)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CadastrarClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(
        CadastrarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CadastrarClienteCommand(
            request.Nome,
            request.Documento,
            request.Email,
            request.Telefone);

        var result = await cadastrarClienteUseCase.ExecutarAsync(
            command,
            cancellationToken);

        var response = new CadastrarClienteResponse(result.Id);

        return Created($"/api/clientes/{result.Id}", response);
    }
}
