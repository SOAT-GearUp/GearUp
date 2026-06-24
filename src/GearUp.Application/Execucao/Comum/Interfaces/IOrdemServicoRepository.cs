using GearUp.Domain.Entities;

namespace GearUp.Application.Execucao.Comum.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct);
}
