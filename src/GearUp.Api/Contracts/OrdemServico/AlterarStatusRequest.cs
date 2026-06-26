using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.OrdemServico
{
    public sealed record AlterarStatusRequest(
        [Required] StatusOrdemServico Status);
}
