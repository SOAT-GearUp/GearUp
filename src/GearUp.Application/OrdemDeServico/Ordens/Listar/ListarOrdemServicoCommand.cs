namespace GearUp.Application.OrdemDeServico.Ordens.Listar;

public sealed record ListarOrdemServicoCommand(bool EmAndamento, Guid? ClienteId);
