using GearUp.Api.Contracts.Estoque;
using GearUp.Application.Estoque.Cadastrar;
using GearUp.Application.Estoque.Listar;
using GearUp.Application.Estoque.Movimentar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers;

[ApiController, Route("api/estoque"), Authorize(Roles = "Admin,Atendente,Auxiliar")]
public sealed class EstoqueController(
    IListarEstoqueItemUseCase listarEstoqueUseCase,
    ICadastrarEstoqueItemUseCase cadastrarEstoqueUseCase,
    IMovimentarEstoqueItemUseCase movimentarEstoqueItemUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<ListarEstoqueItemResult>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var itens = await listarEstoqueUseCase.ListarAsync(ct);
        return Ok(itens);
    }

    [HttpPost]
    [ProducesResponseType<CadastrarEstoqueItemResult>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(CriarItemEstoqueRequest request, CancellationToken ct)
    { 
        var item = await cadastrarEstoqueUseCase.CadastrarAsync(
            new CadastrarEstoqueItemCommand(
                request.Nome, 
                request.Tipo, 
                request.PrecoUnitario,
                request.QuantidadeInicial), ct); 

        return Created($"/api/estoque/{item.Id}", item); 
    }

    [HttpPost("{id:guid}/movimentacoes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Movimentar(Guid id, MovimentarEstoqueRequest request, CancellationToken ct)
    { 
        await movimentarEstoqueItemUseCase.MovimentarAsync(
            new MovimentarEstoqueItemCommand(id, request.Tipo, request.Quantidade, request.Motivo), ct);

        return NoContent(); 
    }
}
