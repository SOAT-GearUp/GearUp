using GearUp.Domain.Enums;

namespace GearUp.Application.Notificacoes.Listar
{
    public sealed record ListarNotificaoCommand(
        DestinatarioNotificacao Destinatario, 
        Guid? ClienteId);
}
