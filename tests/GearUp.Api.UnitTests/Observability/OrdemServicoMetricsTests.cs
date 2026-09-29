using System.Diagnostics.Metrics;
using GearUp.Api.Observability;
using GearUp.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace GearUp.Api.UnitTests.Observability;

public sealed class OrdemServicoMetricsTests
{
    [Fact]
    public void RegistrarEventos_DevePublicarMetricasDeNegocio()
    {
        long criadas = 0;
        long transicoes = 0;
        double tempoStatus = -1;

        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == "GearUp.Api")
                    meterListener.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "gearup.ordens_servico.criadas")
                criadas += measurement;
            else if (instrument.Name == "gearup.ordens_servico.transicoes_status")
                transicoes += measurement;
        });
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "gearup.ordens_servico.tempo_status")
                tempoStatus = measurement;
        });
        listener.Start();

        using var metrics = new OrdemServicoMetrics(NullLogger<OrdemServicoMetrics>.Instance);
        metrics.RegistrarCriacao(Guid.NewGuid(), DateTimeOffset.UtcNow);
        metrics.RegistrarTransicaoStatus(
            Guid.NewGuid(),
            StatusOrdemServico.Recebida,
            StatusOrdemServico.EmDiagnostico,
            TimeSpan.FromSeconds(42),
            DateTimeOffset.UtcNow);

        Assert.Equal(1, criadas);
        Assert.Equal(1, transicoes);
        Assert.Equal(42, tempoStatus);
    }
}
