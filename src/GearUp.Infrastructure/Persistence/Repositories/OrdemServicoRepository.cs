using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

using IAtendimentoRepo = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrdemServicoRepository;
using IDiagnosticoRepo = GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces.IOrdemServicoRepository;
using IExecucaoRepo = GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces.IOrdemServicoRepository;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class OrdemServicoRepository(GearUpDbContext db) : IAtendimentoRepo, IDiagnosticoRepo, IExecucaoRepo
{
    public async Task AdicionarAsync(OrdemServico ordem, CancellationToken ct)
    {
        await db.OrdensServico.AddAsync(ordem, ct);
    }

    public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct)
    {
        return db.OrdensServico
            .Include(x => x.Historico)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct)
    {
        var query = db.OrdensServico
            .AsNoTracking()
            .Include(x => x.Historico)
            .AsQueryable();

        query = query.Where(x =>
            x.Status != StatusOrdemServico.Finalizada &&
            x.Status != StatusOrdemServico.Entregue);

        if (somenteEmAndamento)
            query = query.Where(x => x.Status != StatusOrdemServico.Cancelada);

        if (clienteId.HasValue)
            query = query.Where(x => x.ClienteId == clienteId.Value);

        return await query
            .OrderBy(x => x.Status == StatusOrdemServico.EmExecucao ? 0 :
                x.Status == StatusOrdemServico.AguardandoAprovacao ? 1 :
                x.Status == StatusOrdemServico.EmDiagnostico ? 2 :
                x.Status == StatusOrdemServico.Recebida ? 3 : 4)
            .ThenBy(x => x.CriadaEm)
            .ToListAsync(ct);
    }
}
