using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Domain.DomainEvents.Notificacoes;
using GearUp.Domain.Entities;

namespace GearUp.Application.Notificacoes.EventHandlers
{
    public sealed class NotificacaoSolicitadaDomainEventHandler(
        IComunicacaoRepository comunicacaoRepository)
        : IDomainEventHandler<NotificacaoSolicitadaDomainEvent>
    {
        public async Task HandleAsync(
            NotificacaoSolicitadaDomainEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var comunicacao = Comunicacao.Criar(
                domainEvent.OrdemServicoId,
                domainEvent.ClienteId,
                domainEvent.Destinatario,
                domainEvent.Mensagem);

            await comunicacaoRepository.AdicionarAsync(comunicacao, cancellationToken);
        }
    }
}
