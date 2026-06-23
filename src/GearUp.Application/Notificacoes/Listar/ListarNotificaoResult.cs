using GearUp.Domain.Enums;

namespace GearUp.Application.Notificacoes.Listar
{
    public sealed record ListarNotificaoResult(
        Guid Id,
        Guid OrdemServicoId,
        Guid ClienteId,
        DestinatarioNotificacao Destinatario,
        string Mensagem,
        DateTimeOffset CriadaEm,
        DateTimeOffset? LidaEm);
}
