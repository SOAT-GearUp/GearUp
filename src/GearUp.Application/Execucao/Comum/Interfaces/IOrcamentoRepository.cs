using GearUp.Domain.Entities;

namespace GearUp.Application.Execucao.Comum.Interfaces;

public interface IOrcamentoRepository
{
    Task<Orcamento?> ObterAprovadoPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct);
}
