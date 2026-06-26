using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Domain.DomainEvents.Notificacoes;
using NotificacaoEntity = GearUp.Domain.Entities.Notificacao;

namespace GearUp.Application.Comunicacao.EventHandlers
{
    public sealed class NotificacaoSolicitadaDomainEventHandler(
        INotificacaoRepository notificacaoRepository)
        : IDomainEventHandler<NotificacaoSolicitadaDomainEvent>
    {
        public async Task HandleAsync(
            NotificacaoSolicitadaDomainEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var notificacao = NotificacaoEntity.Criar(
                domainEvent.OrdemServicoId,
                domainEvent.ClienteId,
                domainEvent.Destinatario,
                domainEvent.Mensagem);

            await notificacaoRepository.AdicionarAsync(notificacao, cancellationToken);
        }
    }
}
