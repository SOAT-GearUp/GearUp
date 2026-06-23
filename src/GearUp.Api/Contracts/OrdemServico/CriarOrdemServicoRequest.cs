using GearUp.Domain.Enums;

namespace GearUp.Api.Contracts.OrdemServico
{
    public sealed record CriarOrdemServicoRequest(
        Guid ClienteId, 
        Guid VeiculoId, 
        string SolicitacaoInicial, 
        PrioridadeOrdemServico Prioridade, 
        DateTimeOffset? Prazo);
}
