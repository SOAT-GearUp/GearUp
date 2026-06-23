using GearUp.Application.OrdensServico.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.OrdensServico.Listar
{
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
}
