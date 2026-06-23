using GearUp.Domain.Enums;

namespace GearUp.Api.Contracts.Estoque
{
    public sealed record CriarItemEstoqueRequest(
        string Nome, 
        TipoItemEstoque Tipo, 
        decimal PrecoUnitario,
        decimal QuantidadeInicial = 0);
}
