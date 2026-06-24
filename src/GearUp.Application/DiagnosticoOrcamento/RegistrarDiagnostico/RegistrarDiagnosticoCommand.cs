namespace GearUp.Application.DiagnosticoOrcamento.RegistrarDiagnostico;

public sealed record RegistrarDiagnosticoCommand(Guid OrdemServicoId, string Diagnostico);
