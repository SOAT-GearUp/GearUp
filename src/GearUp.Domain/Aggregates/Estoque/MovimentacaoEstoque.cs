using GearUp.Domain.Enums;

namespace GearUp.Domain.Entities;

public sealed class MovimentacaoEstoque
{
    private MovimentacaoEstoque() { }
    private MovimentacaoEstoque(Guid estoqueItemId, TipoMovimentacaoEstoque tipo, decimal quantidade, string motivo, Guid? ordemServicoId)
    {
        Id = Guid.NewGuid();
        EstoqueItemId = estoqueItemId;
        Tipo = tipo;
        Quantidade = quantidade;
        Motivo = motivo;
        OrdemServicoId = ordemServicoId;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid EstoqueItemId { get; private set; }
    public TipoMovimentacaoEstoque Tipo { get; private set; }
    public decimal Quantidade { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public Guid? OrdemServicoId { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    internal static MovimentacaoEstoque Criar(Guid itemId, TipoMovimentacaoEstoque tipo, decimal quantidade, string motivo, Guid? osId)
    {
        return new(itemId, tipo, quantidade, motivo, osId);
    }
}
