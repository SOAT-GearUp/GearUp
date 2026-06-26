using GearUp.Domain.Entities;

namespace GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct);
}
