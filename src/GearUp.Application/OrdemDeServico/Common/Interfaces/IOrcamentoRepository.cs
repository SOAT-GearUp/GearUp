using GearUp.Domain.Entities;

namespace GearUp.Application.OrdemDeServico.Common.Interfaces;

public interface IOrcamentoRepository
{
    Task<IReadOnlyList<Orcamento>> ListarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct);
}
