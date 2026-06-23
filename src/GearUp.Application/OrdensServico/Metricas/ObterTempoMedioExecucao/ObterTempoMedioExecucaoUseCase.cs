using GearUp.Application.OrdensServico.Common.Interfaces;

namespace GearUp.Application.OrdensServico.Metricas.ObterTempoMedioExecucao
{
    internal sealed class ObterTempoMedioExecucaoUseCase(IOrdemServicoRepository ordemServicoRepository) : IObterTempoMedioExecucaoUseCase
    {
        public async Task<ObterTempoMedioExecucaoResult?> ObterTempoMedioExecucaoAsync(CancellationToken ct)
        {
            var ordensServico = await ordemServicoRepository.ListarAsync(false, null, ct);

            var concluidas = ordensServico
                .Where(x => x.IniciadaEm.HasValue && x.FinalizadaEm.HasValue)
                .ToList();
            
            return concluidas.Count == 0 
                ? null 
                : new ObterTempoMedioExecucaoResult(TimeSpan.FromTicks((long)concluidas.Average(x => (x.FinalizadaEm!.Value - x.IniciadaEm!.Value).Ticks)));
        }
    }
}
