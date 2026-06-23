using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Domain.DomainEvents.Notificacoes;
using GearUp.Domain.Entities;

namespace GearUp.Application.Notificacoes.EventHandlers
{
    public sealed class NotificacaoSolicitadaDomainEventHandler(
        INotificacaoRepository notificacaoRepository)
        : IDomainEventHandler<NotificacaoSolicitadaDomainEvent>
    {
        public async Task HandleAsync(
            NotificacaoSolicitadaDomainEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var notificacao = Notificacao.Criar(
                domainEvent.OrdemServicoId,
                domainEvent.ClienteId,
                domainEvent.Destinatario,
                domainEvent.Mensagem);

            await notificacaoRepository.AdicionarAsync(
                notificacao,
                cancellationToken);
        }
    }
}
