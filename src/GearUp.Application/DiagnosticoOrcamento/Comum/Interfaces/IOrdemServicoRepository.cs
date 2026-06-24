using GearUp.Domain.Entities;

namespace GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct);
}
