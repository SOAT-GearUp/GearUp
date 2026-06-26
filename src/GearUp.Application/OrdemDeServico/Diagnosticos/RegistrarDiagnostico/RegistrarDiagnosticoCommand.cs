namespace GearUp.Application.OrdemDeServico.Diagnosticos.RegistrarDiagnostico;

public sealed record RegistrarDiagnosticoCommand(Guid OrdemServicoId, string Diagnostico);
