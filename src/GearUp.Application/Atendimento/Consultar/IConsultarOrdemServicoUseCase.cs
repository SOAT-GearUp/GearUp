namespace GearUp.Application.Atendimento.Consultar;

public interface IConsultarOrdemServicoUseCase
{
    Task<ConsultarOrdemServicoResult> ObterAsync(ConsultarOrdemServicoCommand command, CancellationToken ct);
}
