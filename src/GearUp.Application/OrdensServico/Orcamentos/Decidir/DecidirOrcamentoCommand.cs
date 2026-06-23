using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Orcamentos.Decidir
{
    public sealed record DecidirOrcamentoCommand(
        Guid OrdemServicoId,
        Guid OrcamentoId,
        bool Aprovado);
}
