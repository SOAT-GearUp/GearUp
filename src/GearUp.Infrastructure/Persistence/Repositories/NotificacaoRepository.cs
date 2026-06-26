using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories
{
    internal sealed class NotificacaoRepository(GearUpDbContext db) : INotificacaoRepository
    {
        public Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken)
        {
            return db.Notificacoes.AddAsync(notificacao, cancellationToken).AsTask();
        }

        public async Task<IReadOnlyList<Notificacao>> ListarAsync(DestinatarioNotificacao destinatario, Guid? clienteId, CancellationToken ct)
        {
            var query = db.Notificacoes
                .AsNoTracking()
                .Where(x => x.Destinatario == destinatario);

            if (clienteId.HasValue)
                query = query.Where(x => x.ClienteId == clienteId.Value);

            return await query
                .OrderByDescending(x => x.CriadaEm)
                .ToListAsync(ct);
        }
    }
}
