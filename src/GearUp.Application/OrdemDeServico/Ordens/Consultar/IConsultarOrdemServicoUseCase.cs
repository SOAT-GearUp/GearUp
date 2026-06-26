namespace GearUp.Application.OrdemDeServico.Ordens.Consultar;

public interface IConsultarOrdemServicoUseCase
{
    Task<ConsultarOrdemServicoResult> ObterAsync(ConsultarOrdemServicoCommand command, CancellationToken ct);
}
