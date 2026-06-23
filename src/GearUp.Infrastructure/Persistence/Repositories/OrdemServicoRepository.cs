using GearUp.Application.OrdensServico.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class OrdemServicoRepository(GearUpDbContext db) : IOrdemServicoRepository
{
    public async Task AdicionarAsync(OrdemServico ordem, CancellationToken ct)
    {
        await db.OrdensServico.AddAsync(ordem, ct);
    }

    public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct)
    {
        return db.OrdensServico
            .Include(x => x.Orcamentos)
            .ThenInclude(x => x.Itens)
            .Include(x => x.Historico)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<OrdemServico>> ListarAsync(bool andamento, Guid? clienteId, CancellationToken ct)
    {
        var query = db.OrdensServico
            .AsNoTracking()
            .Include(x => x.Orcamentos)
            .ThenInclude(x => x.Itens)
            .Include(x => x.Historico)
            .AsQueryable();

        if (andamento) 
            query = query.Where(x => x.Status != StatusOrdemServico.Entregue && x.Status != StatusOrdemServico.Cancelada);

        if (clienteId.HasValue) 
            query = query.Where(x => x.ClienteId == clienteId.Value);

        return await query
            .OrderByDescending(x => x.Prioridade)
            .ThenBy(x => x.Prazo)
            .ThenBy(x => x.CriadaEm)
            .ToListAsync(ct);
    }

}
