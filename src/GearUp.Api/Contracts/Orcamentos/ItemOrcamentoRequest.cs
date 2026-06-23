using GearUp.Domain.Enums;

namespace GearUp.Api.Contracts.Orcamentos
{
    public sealed record ItemOrcamentoRequest(
        TipoItemOrcamento Tipo, 
        string Descricao, 
        decimal Quantidade, 
        decimal ValorUnitario, 
        Guid? EstoqueItemId);
}
