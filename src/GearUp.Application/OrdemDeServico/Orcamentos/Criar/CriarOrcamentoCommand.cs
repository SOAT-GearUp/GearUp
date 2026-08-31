namespace GearUp.Application.OrdemDeServico.Orcamentos.Criar;

public sealed record CriarOrcamentoCommand(
    Guid OrdemServicoId,
    IReadOnlyCollection<CriarItemOrcamentoCommand> Itens);
