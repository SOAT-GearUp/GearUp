using GearUp.Domain.Entities;

namespace GearUp.Application.Notificacoes.Common.Interfaces;

public interface IComunicacaoRepository
{
    Task<IReadOnlyList<Comunicacao>> ListarAsync(DestinatarioNotificacao destinatario, Guid? clienteId, CancellationToken ct);
    Task AdicionarAsync(Comunicacao comunicacao, CancellationToken cancellationToken);
}
