namespace GearUp.Application.DiagnosticoOrcamento.IniciarDiagnostico;

public sealed record IniciarDiagnosticoCommand(Guid OrdemServicoId, Guid MecanicoId);
