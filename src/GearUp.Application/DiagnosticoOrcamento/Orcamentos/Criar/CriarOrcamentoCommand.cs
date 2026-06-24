namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Criar;

public sealed record CriarOrcamentoCommand(
    Guid OrdemServicoId,
    IReadOnlyCollection<CriarItemOrcamentoCommand> Itens);
