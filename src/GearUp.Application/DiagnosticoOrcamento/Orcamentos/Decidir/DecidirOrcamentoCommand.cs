namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Decidir;

public sealed record DecidirOrcamentoCommand(Guid OrdemServicoId, Guid OrcamentoId, bool Aprovado);
