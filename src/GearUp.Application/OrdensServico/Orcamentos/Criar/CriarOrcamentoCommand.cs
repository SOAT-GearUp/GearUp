namespace GearUp.Application.OrdensServico.Orcamentos.Criar
{
    public sealed record CriarOrcamentoCommand(
        Guid OrdemServicoId,
        IReadOnlyCollection<CriarItemOrcamentoCommand> Itens);
}