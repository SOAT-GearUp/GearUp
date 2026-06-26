using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Domain.DomainEvents.DiagnosticoOrcamento;
using GearUp.Domain.Enums;
using NotificacaoEntity = GearUp.Domain.Entities.Notificacao;

namespace GearUp.Application.Comunicacao.EventHandlers;

public sealed class OrcamentoDisponivelDomainEventHandler(
    INotificacaoRepository notificacaoRepository)
    : IDomainEventHandler<OrcamentoDisponivelDomainEvent>
{
    public async Task HandleAsync(
        OrcamentoDisponivelDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var notificacao = NotificacaoEntity.Criar(
            domainEvent.OrdemServicoId,
            domainEvent.ClienteId,
            DestinatarioNotificacao.Cliente,
            $"O orçamento v{domainEvent.Versao} está disponível para aprovação.");

        await notificacaoRepository.AdicionarAsync(notificacao, cancellationToken);
    }
}
