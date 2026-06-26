using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Orcamentos
{
    public sealed record DecisaoOrcamentoRequest(
        [Required] bool Aprovado);
}
