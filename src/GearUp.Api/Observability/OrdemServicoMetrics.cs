using System.Diagnostics;
using System.Diagnostics.Metrics;
using GearUp.Application.Common.Observability;
using GearUp.Domain.Enums;

namespace GearUp.Api.Observability;

internal sealed class OrdemServicoMetrics : IOrdemServicoMetrics, IDisposable
{
    private readonly ILogger<OrdemServicoMetrics> logger;
    private readonly Meter meter = new(ObservabilityConstants.MeterName);
    private readonly Counter<long> created;
    private readonly Counter<long> statusTransitions;
    private readonly Histogram<double> statusDuration;

    public OrdemServicoMetrics(ILogger<OrdemServicoMetrics> logger)
    {
        this.logger = logger;
        created = meter.CreateCounter<long>(
            "gearup.ordens_servico.criadas",
            description: "Quantidade de ordens de serviço criadas.");
        statusTransitions = meter.CreateCounter<long>(
            "gearup.ordens_servico.transicoes_status",
            description: "Quantidade de transições de status das ordens de serviço.");
        statusDuration = meter.CreateHistogram<double>(
            "gearup.ordens_servico.tempo_status",
            unit: "s",
            description: "Tempo permanecido pela ordem de serviço no status anterior.");
    }

    public void RegistrarCriacao(Guid ordemServicoId, DateTimeOffset ocorridoEm)
    {
        created.Add(1, new KeyValuePair<string, object?>("gearup.status", StatusOrdemServico.Recebida.ToString()));

        logger.LogInformation(
            "Ordem de serviço {OrdemServicoId} criada em {OcorridoEm}.",
            ordemServicoId,
            ocorridoEm);
    }

    public void RegistrarTransicaoStatus(
        Guid ordemServicoId,
        StatusOrdemServico statusAnterior,
        StatusOrdemServico statusAtual,
        TimeSpan tempoNoStatusAnterior,
        DateTimeOffset ocorridoEm)
    {
        if (statusAnterior == statusAtual)
            return;

        var tags = new TagList
        {
            { "gearup.status.anterior", statusAnterior.ToString() },
            { "gearup.status.atual", statusAtual.ToString() }
        };

        statusTransitions.Add(1, tags);
        statusDuration.Record(Math.Max(0, tempoNoStatusAnterior.TotalSeconds), tags);

        Activity.Current?.AddEvent(new ActivityEvent(
            "gearup.ordem_servico.status_alterado",
            tags: new ActivityTagsCollection
            {
                { "gearup.status.anterior", statusAnterior.ToString() },
                { "gearup.status.atual", statusAtual.ToString() }
            }));

        logger.LogInformation(
            "Ordem de serviço {OrdemServicoId} alterada de {StatusAnterior} para {StatusAtual} em {OcorridoEm} após {DuracaoSegundos} segundos.",
            ordemServicoId, statusAnterior, statusAtual, ocorridoEm, tempoNoStatusAnterior.TotalSeconds);
    }

    public void Dispose() => meter.Dispose();
}
