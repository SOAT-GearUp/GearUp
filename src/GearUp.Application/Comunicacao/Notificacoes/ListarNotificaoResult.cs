using GearUp.Domain.Enums;

namespace GearUp.Application.Comunicacao.Notificacoes
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
