using GearUp.Domain.Entities;

namespace GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct);
}
