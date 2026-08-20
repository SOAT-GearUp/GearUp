namespace GearUp.Application.OrdemDeServico.Ordens.ConsultarStatus;

public interface IConsultarStatusOrdemServicoUseCase
{
    Task<ConsultarStatusOrdemServicoResult> ObterAsync(ConsultarStatusOrdemServicoCommand command, CancellationToken ct);
}
