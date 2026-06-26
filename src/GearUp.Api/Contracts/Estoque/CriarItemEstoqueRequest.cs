using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Estoque
{
    public sealed record CriarItemEstoqueRequest(
        [Required] string Nome, 
        [Required] TipoItemEstoque Tipo, 
        [Required] decimal PrecoUnitario,
        [Required] decimal QuantidadeInicial = 0);
}
