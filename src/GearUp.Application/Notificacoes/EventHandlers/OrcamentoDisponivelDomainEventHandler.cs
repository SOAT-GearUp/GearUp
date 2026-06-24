using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Domain.DomainEvents.DiagnosticoOrcamento;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.Notificacoes.EventHandlers;

public sealed class OrcamentoDisponivelDomainEventHandler(
    INotificacaoRepository notificacaoRepository)
    : IDomainEventHandler<OrcamentoDisponivelDomainEvent>
{
    public async Task HandleAsync(
        OrcamentoDisponivelDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var notificacao = Notificacao.Criar(
            domainEvent.OrdemServicoId,
            domainEvent.ClienteId,
            DestinatarioNotificacao.Cliente,
            $"O orçamento v{domainEvent.Versao} está disponível para aprovação.");

        await notificacaoRepository.AdicionarAsync(notificacao, cancellationToken);
    }
}
