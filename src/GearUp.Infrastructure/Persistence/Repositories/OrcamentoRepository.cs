using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

using IAtendimentoOrcamentoRepo = GearUp.Application.Atendimento.Comum.Interfaces.IOrcamentoRepository;
using IDiagnosticoOrcamentoRepo = GearUp.Application.DiagnosticoOrcamento.Orcamentos.Common.Interfaces.IOrcamentoRepository;
using IExecucaoOrcamentoRepo = GearUp.Application.Execucao.Comum.Interfaces.IOrcamentoRepository;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class OrcamentoRepository(GearUpDbContext db)
    : IAtendimentoOrcamentoRepo, IDiagnosticoOrcamentoRepo, IExecucaoOrcamentoRepo
{
    public async Task AdicionarAsync(Orcamento orcamento, CancellationToken ct) =>
        await db.Orcamentos.AddAsync(orcamento, ct);

    public Task<Orcamento?> ObterAsync(Guid id, CancellationToken ct) =>
        db.Orcamentos.Include(x => x.Itens).SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Orcamento>> ListarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) =>
        await db.Orcamentos.Include(x => x.Itens)
            .Where(x => x.OrdemServicoId == ordemServicoId)
            .OrderBy(x => x.Versao)
            .ToListAsync(ct);

    public Task<int> ContarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) =>
        db.Orcamentos.CountAsync(x => x.OrdemServicoId == ordemServicoId, ct);

    public Task<Orcamento?> ObterAprovadoPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) =>
        db.Orcamentos.Include(x => x.Itens)
            .SingleOrDefaultAsync(x => x.OrdemServicoId == ordemServicoId && x.Status == StatusOrcamento.Aprovado, ct);
}
