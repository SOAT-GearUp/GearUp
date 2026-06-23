using GearUp.Api.Controllers;

namespace GearUp.Api.Contracts.Orcamentos
{
    public sealed record CriarOrcamentoRequest(
        IReadOnlyCollection<ItemOrcamentoRequest> Itens);
}
