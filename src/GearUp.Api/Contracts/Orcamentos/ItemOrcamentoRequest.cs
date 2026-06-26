using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Orcamentos
{
    public sealed record ItemOrcamentoRequest(
        [Required] TipoItemOrcamento Tipo,
        [Required] string Descricao, 
        [Required] decimal Quantidade, 
        [Required] decimal ValorUnitario, 
        Guid? EstoqueItemId);
}
