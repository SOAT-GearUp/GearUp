using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Execucao.AlterarStatus;

public sealed record AlterarStatusCommand(Guid Id, StatusOrdemServico Status);
