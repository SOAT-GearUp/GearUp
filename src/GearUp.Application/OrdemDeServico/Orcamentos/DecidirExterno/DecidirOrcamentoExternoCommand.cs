namespace GearUp.Application.OrdemDeServico.Orcamentos.DecidirExterno;

public sealed record DecidirOrcamentoExternoCommand(Guid OrcamentoId, bool Aprovado);
