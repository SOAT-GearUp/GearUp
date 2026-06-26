using NotificacaoEntity = GearUp.Domain.Entities.Notificacao;

namespace GearUp.Application.Comunicacao.Common.Interfaces;

public interface INotificacaoRepository
{
    Task<IReadOnlyList<NotificacaoEntity>> ListarAsync(DestinatarioNotificacao destinatario, Guid? clienteId, CancellationToken ct);
    Task AdicionarAsync(NotificacaoEntity notificacao, CancellationToken cancellationToken);
}
