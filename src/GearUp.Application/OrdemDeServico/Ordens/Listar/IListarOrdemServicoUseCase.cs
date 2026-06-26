namespace GearUp.Application.OrdemDeServico.Ordens.Listar;

public interface IListarOrdemServicoUseCase
{
    Task<IReadOnlyList<ListarOrdemServicoResult>> ListarAsync(ListarOrdemServicoCommand command, CancellationToken ct);
}
