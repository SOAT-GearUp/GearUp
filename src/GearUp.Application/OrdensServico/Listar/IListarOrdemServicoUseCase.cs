using GearUp.Domain.Entities;

namespace GearUp.Application.OrdensServico.Listar
{
    public interface IListarOrdemServicoUseCase
    {
        Task<IReadOnlyList<ListarOrdemServicoResult>> ListarAsync(ListarOrdemServicoCommand command, CancellationToken ct);
    }
}
