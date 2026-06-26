namespace GearUp.Application.OrdemDeServico.Execucao.Metricas;

public interface IObterTempoMedioExecucaoUseCase
{
    Task<ObterTempoMedioExecucaoResult?> ObterTempoMedioExecucaoAsync(CancellationToken ct);
}
