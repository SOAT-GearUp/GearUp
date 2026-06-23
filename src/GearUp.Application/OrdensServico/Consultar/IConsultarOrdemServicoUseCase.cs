using GearUp.Domain.Entities;

namespace GearUp.Application.OrdensServico.Consultar
{
    public interface IConsultarOrdemServicoUseCase
    {
        Task<ConsultarOrdemServicoResult> ObterAsync(ConsultarOrdemServicoCommand command, CancellationToken ct);
    }
}
