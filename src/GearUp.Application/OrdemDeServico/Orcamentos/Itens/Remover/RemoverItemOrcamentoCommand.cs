namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Remover;

public sealed record RemoverItemOrcamentoCommand(Guid OrdemServicoId, Guid OrcamentoId, Guid ItemId);
