using GearUp.Domain.Entities;

namespace GearUp.Application.OrdensServico.Common.Interfaces
{
    public interface IOrdemServicoRepository
    {
        Task AdicionarAsync(OrdemServico ordem, CancellationToken ct);
        Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct);
    }
}