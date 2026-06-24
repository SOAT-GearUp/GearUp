namespace GearUp.Application.Execucao.Metricas;

public interface IObterTempoMedioExecucaoUseCase
{
    Task<ObterTempoMedioExecucaoResult?> ObterTempoMedioExecucaoAsync(CancellationToken ct);
}
