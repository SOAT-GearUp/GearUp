using GearUp.Domain.Entities;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Common.Interfaces;

public interface IOrcamentoRepository
{
    Task AdicionarAsync(Orcamento orcamento, CancellationToken ct);
    Task<Orcamento?> ObterAsync(Guid id, CancellationToken ct);
    Task<int> ContarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct);
}
