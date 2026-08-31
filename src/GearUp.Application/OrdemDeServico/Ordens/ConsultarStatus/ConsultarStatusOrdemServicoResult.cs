using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Ordens.ConsultarStatus;

public sealed record ConsultarStatusOrdemServicoResult(
    Guid OrdemServicoId,
    Guid ClienteId,
    StatusOrdemServico Status);
