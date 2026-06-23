namespace GearUp.Application.OrdensServico.Metricas.ObterTempoMedioExecucao
{
    public interface IObterTempoMedioExecucaoUseCase
    {
        Task<ObterTempoMedioExecucaoResult?> ObterTempoMedioExecucaoAsync(CancellationToken ct);
    }
}
