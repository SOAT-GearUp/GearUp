using GearUp.Domain.Enums;

namespace GearUp.Application.Comunicacao.Notificacoes
{
    public sealed record ListarNotificaoCommand(
        DestinatarioNotificacao Destinatario, 
        Guid? ClienteId);
}
