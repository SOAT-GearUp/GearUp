using GearUp.Application.OrdensServico.Common;
using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Consultar
{
    public sealed record ConsultarOrdemServicoResult(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string SolicitacaoInicial,
    string? Diagnostico,
    StatusOrdemServico Status,
    PrioridadeOrdemServico Prioridade,
    DateTimeOffset? Prazo,
    DateTimeOffset CriadaEm,
    DateTimeOffset? IniciadaEm,
    DateTimeOffset? FinalizadaEm,
    IReadOnlyCollection<OrcamentoResult> Orcamentos,
    IReadOnlyCollection<HistoricoOrdemServicoResult> Historico);
}
