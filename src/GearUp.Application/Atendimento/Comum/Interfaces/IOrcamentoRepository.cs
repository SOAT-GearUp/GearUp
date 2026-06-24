using GearUp.Domain.Entities;

namespace GearUp.Application.Atendimento.Comum.Interfaces;

public interface IOrcamentoRepository
{
    Task<IReadOnlyList<Orcamento>> ListarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct);
}
