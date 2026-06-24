using GearUp.Domain.Enums;

namespace GearUp.Application.Execucao.AlterarStatus;

public sealed record AlterarStatusCommand(Guid Id, StatusOrdemServico Status);
