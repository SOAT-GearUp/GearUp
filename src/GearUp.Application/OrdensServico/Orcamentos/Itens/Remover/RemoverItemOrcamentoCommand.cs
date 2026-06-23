namespace GearUp.Application.OrdensServico.Orcamentos.Itens.Remover
{
    public sealed record RemoverItemOrcamentoCommand(
        Guid OrdemServicoId,
        Guid OrcamentoId,
        Guid ItemId
    );
}
