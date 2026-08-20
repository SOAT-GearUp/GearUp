using GearUp.Application.OrdemDeServico.Orcamentos.Criar;
using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Ordens.Criar;

public sealed record CriarOrdemServicoCommand(
    Guid ClienteId,
    Guid VeiculoId,
    string SolicitacaoInicial,
    PrioridadeOrdemServico Prioridade,
    DateTimeOffset? Prazo,
    IReadOnlyCollection<CriarItemOrcamentoCommand>? Itens);
