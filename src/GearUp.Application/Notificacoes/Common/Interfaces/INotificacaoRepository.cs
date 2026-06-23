using GearUp.Domain.Entities;

namespace GearUp.Application.Notificacoes.Common.Interfaces
{
    public interface INotificacaoRepository
    {
        Task<IReadOnlyList<Notificacao>> ListarAsync(DestinatarioNotificacao destinatario, Guid? clienteId, CancellationToken ct);
        Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken);
    }
}
