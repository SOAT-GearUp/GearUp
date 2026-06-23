using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Criar
{
    public sealed record CriarOrdemServicoCommand(
        Guid ClienteId,
        Guid VeiculoId,
        string SolicitacaoInicial,
        PrioridadeOrdemServico Prioridade,
        DateTimeOffset? Prazo);
}
