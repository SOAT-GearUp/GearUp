using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Status.Alterar
{
    public sealed record AlterarStatusCommand(
        Guid Id, 
        StatusOrdemServico Status);
}
