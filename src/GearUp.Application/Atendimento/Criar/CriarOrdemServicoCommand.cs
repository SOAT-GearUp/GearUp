using GearUp.Domain.Enums;

namespace GearUp.Application.Atendimento.Criar;

public sealed record CriarOrdemServicoCommand(
    Guid ClienteId,
    Guid VeiculoId,
    string SolicitacaoInicial,
    PrioridadeOrdemServico Prioridade,
    DateTimeOffset? Prazo);
