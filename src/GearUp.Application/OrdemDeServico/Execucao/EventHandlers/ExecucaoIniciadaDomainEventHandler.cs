using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.DomainEvents.Execucao;
using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Execucao.EventHandlers;

public sealed class ExecucaoIniciadaDomainEventHandler(
    IEstoqueRepository estoqueRepository)
    : IDomainEventHandler<ExecucaoIniciadaDomainEvent>
{
    public async Task HandleAsync(ExecucaoIniciadaDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        foreach (var item in domainEvent.ItensParaDeduzir)
        {
            var estoque = await estoqueRepository.ObterAsync(item.EstoqueItemId, cancellationToken)
                ?? throw new RecursoNaoEncontradoException("ITEM_ESTOQUE_NAO_ENCONTRADO", $"Item '{item.Descricao}' não encontrado no estoque.");

            estoque.Movimentar(TipoMovimentacaoEstoque.Saida, item.Quantidade, $"Consumo automático na OS {domainEvent.OrdemServicoId}", domainEvent.OrdemServicoId);
        }
    }
}
