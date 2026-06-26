namespace GearUp.Application.OrdemDeServico.Diagnosticos.IniciarDiagnostico;

public sealed record IniciarDiagnosticoCommand(Guid OrdemServicoId, Guid MecanicoId);
