using GearUp.Domain.Common.DomainEvents;
using GearUp.Domain.Enums;

namespace GearUp.Domain.DomainEvents.Notificacoes
{
    public sealed record NotificacaoSolicitadaDomainEvent(
        Guid OrdemServicoId,
        Guid ClienteId,
        DestinatarioNotificacao Destinatario,
        string Mensagem,
        DateTimeOffset OcorridoEm)
        : IDomainEvent;
}
