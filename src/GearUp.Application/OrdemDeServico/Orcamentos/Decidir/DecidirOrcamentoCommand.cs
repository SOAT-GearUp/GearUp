namespace GearUp.Application.OrdemDeServico.Orcamentos.Decidir;

public sealed record DecidirOrcamentoCommand(Guid OrdemServicoId, Guid OrcamentoId, bool Aprovado);
