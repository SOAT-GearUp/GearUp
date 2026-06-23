using GearUp.Domain.Enums;

namespace GearUp.Api.Contracts.Orcamentos
{
    public sealed record AlterarStatusRequest(
        StatusOrdemServico Status);
}
