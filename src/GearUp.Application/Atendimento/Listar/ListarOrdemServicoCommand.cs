namespace GearUp.Application.Atendimento.Listar;

public sealed record ListarOrdemServicoCommand(bool EmAndamento, Guid? ClienteId);
