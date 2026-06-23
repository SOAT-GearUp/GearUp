using GearUp.Api.Contracts.Orcamentos;
using GearUp.Application.OrdensServico.Consultar;
using GearUp.Application.OrdensServico.Orcamentos.Criar;
using GearUp.Application.OrdensServico.Orcamentos.Decidir;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GearUp.Api.Authorization;
using GearUp.Application.OrdensServico.Orcamentos.Itens.Adicionar;
using GearUp.Application.OrdensServico.Orcamentos.Itens.Atualizar;
using GearUp.Application.OrdensServico.Orcamentos.Itens.Remover;

namespace GearUp.Api.Controllers
{
    [Route("api/ordens-servico/{ordemServicoId:guid}/orcamentos")]
    [ApiController]
    public class OrcamentosController(
        ICriarOrcamentoUseCase criarOrcamentoUseCase,
        IDecidirOrcamentoUseCase decidirOrcamentoUseCase,
        IConsultarOrdemServicoUseCase consultarOrdemServicoUseCase,
        IAdicionarItemOrcamentoUseCase adicionarItemOrcamentoUseCase,
        IAtualizarItemOrcamentoUseCase atualizarItemOrcamentoUseCase,
        IRemoverItemOrcamentoUseCase removerItemOrcamentoUseCase) : ControllerBase
    {
        [HttpPost, Authorize(Roles = "Atendente,Mecanico")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CriarOrcamento(Guid ordemServicoId, CriarOrcamentoRequest request, CancellationToken ct)
        {
            var itens = request.Itens.Select(i
                => new CriarItemOrcamentoCommand(i.Tipo, i.Descricao, i.Quantidade, i.ValorUnitario, i.EstoqueItemId)
            ).ToList();

            var orcamento = await criarOrcamentoUseCase.CriarAsync(
                new CriarOrcamentoCommand(ordemServicoId, itens
            ), ct);

            return Created($"/api/ordens-servico/{ordemServicoId}/orcamentos/{orcamento.Id}", orcamento);
        }


        [HttpPost("{orcamentoId:guid}/decisao"), Authorize(Roles = "Cliente,Atendente")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Decidir(Guid ordemServicoId, Guid orcamentoId, DecisaoOrcamentoRequest request, CancellationToken ct)
        {
            var os = await consultarOrdemServicoUseCase.ObterAsync(
                new ConsultarOrdemServicoCommand(ordemServicoId
            ), ct);

            // NotFound() para não revelar que a ordem de outro cliente existe
            if (!User.PodeAcessarOrdemServico(os.ClienteId))
                return NotFound(new
                {
                    code = "OS_NAO_ENCONTRADA",
                    message = "Ordem de serviço não encontrada."
                });

            await decidirOrcamentoUseCase.DecidirAsync(
                new DecidirOrcamentoCommand(ordemServicoId, orcamentoId, request.Aprovado
            ), ct); 

            return NoContent(); 
        }

        [HttpPost("{orcamentoId:guid}/itens"), Authorize(Roles = "Atendente,Mecanico")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdicionarItem(Guid ordemServicoId, Guid orcamentoId, ItemOrcamentoRequest request, CancellationToken ct)
        { 
            await adicionarItemOrcamentoUseCase.AdicionarAsync(
                new AdicionarItemOrcamentoCommand(
                    ordemServicoId, orcamentoId, request.Tipo, request.Descricao, request.Quantidade, request.ValorUnitario, request.EstoqueItemId
            ), ct); 

            return NoContent(); 
        }

        [HttpPut("{orcamentoId:guid}/itens/{itemId:guid}"), Authorize(Roles = "Atendente,Mecanico")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarItem(Guid ordemServicoId, Guid orcamentoId, Guid itemId, ItemOrcamentoRequest request, CancellationToken ct)
        { 
            await atualizarItemOrcamentoUseCase.AtualizarAsync(
                new AtualizarItemOrcamentoCommand(
                    ordemServicoId, orcamentoId, itemId, request.Tipo, request.Descricao, request.Quantidade, request.ValorUnitario, request.EstoqueItemId
            ), ct);

            return NoContent(); 
        }

        [HttpDelete("{orcamentoId:guid}/itens/{itemId:guid}"), Authorize(Roles = "Atendente,Mecanico")]
        public async Task<IActionResult> RemoverItem(Guid ordemServicoId, Guid orcamentoId, Guid itemId, CancellationToken ct)
        { 
            await removerItemOrcamentoUseCase.RemoverAsync(
                new RemoverItemOrcamentoCommand(ordemServicoId, orcamentoId, itemId
            ), ct);

            return NoContent(); 
        }

    }
}
