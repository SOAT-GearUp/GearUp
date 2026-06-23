using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Orcamentos.Itens.Atualizar
{
    public sealed record AtualizarItemOrcamentoCommand(
        Guid OrdemServicoId,
        Guid OrcamentoId,
        Guid ItemId,
        TipoItemOrcamento Tipo,
        string Descricao,
        decimal Quantidade,
        decimal ValorUnitario,
        Guid? EstoqueItemId);
}
