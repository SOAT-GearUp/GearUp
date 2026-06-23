using GearUp.Domain.Enums;

namespace GearUp.Application.Estoque.Cadastrar
{
    public sealed record CadastrarEstoqueItemCommand(
        string Nome,
        TipoItemEstoque Tipo,
        decimal Preco,
        decimal QuantidadeInicial = 0
    );
}
