namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Remover;

public sealed record RemoverItemOrcamentoCommand(Guid OrdemServicoId, Guid OrcamentoId, Guid ItemId);
