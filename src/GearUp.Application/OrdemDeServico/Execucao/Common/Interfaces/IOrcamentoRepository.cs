using GearUp.Domain.Entities;

namespace GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces;

public interface IOrcamentoRepository
{
    Task<Orcamento?> ObterAprovadoPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct);
}
