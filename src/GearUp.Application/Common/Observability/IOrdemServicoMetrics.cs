using GearUp.Domain.Enums;

namespace GearUp.Application.Common.Observability;

public interface IOrdemServicoMetrics
{
    void RegistrarCriacao(Guid ordemServicoId, DateTimeOffset ocorridoEm);

    void RegistrarTransicaoStatus(
        Guid ordemServicoId,
        StatusOrdemServico statusAnterior,
        StatusOrdemServico statusAtual,
        TimeSpan tempoNoStatusAnterior,
        DateTimeOffset ocorridoEm);
}

internal sealed class NullOrdemServicoMetrics : IOrdemServicoMetrics
{
    public void RegistrarCriacao(Guid ordemServicoId, DateTimeOffset ocorridoEm)
    {
    }

    public void RegistrarTransicaoStatus(
        Guid ordemServicoId,
        StatusOrdemServico statusAnterior,
        StatusOrdemServico statusAtual,
        TimeSpan tempoNoStatusAnterior,
        DateTimeOffset ocorridoEm)
    {
    }
}
