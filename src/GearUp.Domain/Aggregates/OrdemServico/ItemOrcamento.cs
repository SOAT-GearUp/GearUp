using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Domain.Entities
{
    public sealed class ItemOrcamento
    {
        private ItemOrcamento() { }
        private ItemOrcamento(Guid orcamentoId, NovoItemOrcamento item)
        {
            if (item.Quantidade <= 0 || item.ValorUnitario < 0)
                throw new ArgumentException("Quantidade e valor do item são inválidos.");

            Id = Guid.NewGuid();
            OrcamentoId = orcamentoId;
            Tipo = item.Tipo;
            Descricao = item.Descricao.Trim();
            Quantidade = item.Quantidade;
            ValorUnitario = decimal.Round(item.ValorUnitario, 2);
            EstoqueItemId = item.EstoqueItemId;
        }

        public Guid Id { get; private set; }
        public Guid OrcamentoId { get; private set; }
        public TipoItemOrcamento Tipo { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public Guid? EstoqueItemId { get; private set; }
        public decimal ValorTotal => Quantidade * ValorUnitario;

        internal static ItemOrcamento Criar(Guid orcamentoId, NovoItemOrcamento item)
            => new(orcamentoId, item);

        internal void Atualizar(NovoItemOrcamento item)
        {
            if (item.Quantidade <= 0 || item.ValorUnitario < 0)
                throw new ArgumentException("Quantidade e valor do item são inválidos.");

            Tipo = item.Tipo;
            Descricao = item.Descricao.Trim();
            Quantidade = item.Quantidade;
            ValorUnitario = decimal.Round(item.ValorUnitario, 2);
            EstoqueItemId = item.EstoqueItemId;
        }
    }
}
