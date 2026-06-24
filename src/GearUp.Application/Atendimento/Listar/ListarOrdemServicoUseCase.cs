using GearUp.Application.Atendimento.Comum.Interfaces;

namespace GearUp.Application.Atendimento.Listar;

internal sealed class ListarOrdemServicoUseCase(IOrdemServicoRepository ordemRepository) : IListarOrdemServicoUseCase
{
    public async Task<IReadOnlyList<ListarOrdemServicoResult>> ListarAsync(ListarOrdemServicoCommand command, CancellationToken ct)
    {
        var ordens = await ordemRepository.ListarAsync(command.EmAndamento, command.ClienteId, ct);

        return ordens.Select(os =>
            new ListarOrdemServicoResult(
                os.Id,
                os.ClienteId,
                os.VeiculoId,
                os.Status,
                os.Prioridade,
                os.CriadaEm,
                os.Prazo)).ToList();
    }
}
