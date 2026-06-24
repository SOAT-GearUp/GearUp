using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Domain.DomainEvents.DiagnosticoOrcamento;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.Notificacoes.EventHandlers;

public sealed class OrcamentoDisponivelDomainEventHandler(
    IComunicacaoRepository comunicacaoRepository)
    : IDomainEventHandler<OrcamentoDisponivelDomainEvent>
{
    public async Task HandleAsync(
        OrcamentoDisponivelDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var comunicacao = Comunicacao.Criar(
            domainEvent.OrdemServicoId,
            domainEvent.ClienteId,
            DestinatarioNotificacao.Cliente,
            $"O orçamento v{domainEvent.Versao} está disponível para aprovação.");

        await comunicacaoRepository.AdicionarAsync(comunicacao, cancellationToken);
    }
}
