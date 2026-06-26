using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Estoque
{
    public sealed record MovimentarEstoqueRequest(
        [Required] TipoMovimentacaoEstoque Tipo,
        [Required] decimal Quantidade, 
        string Motivo);
}
