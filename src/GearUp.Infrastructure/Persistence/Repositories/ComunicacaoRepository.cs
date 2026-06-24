using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories
{
    internal sealed class ComunicacaoRepository(GearUpDbContext db) : IComunicacaoRepository
    {
        public Task AdicionarAsync(Comunicacao comunicacao, CancellationToken cancellationToken)
        {
            return db.Comunicacoes.AddAsync(comunicacao, cancellationToken).AsTask();
        }

        public async Task<IReadOnlyList<Comunicacao>> ListarAsync(DestinatarioNotificacao destinatario, Guid? clienteId, CancellationToken ct)
        {
            var query = db.Comunicacoes
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
