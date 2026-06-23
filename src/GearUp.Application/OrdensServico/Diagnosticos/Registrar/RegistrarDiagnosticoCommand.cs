namespace GearUp.Application.OrdensServico.Diagnosticos.Registrar
{
    public sealed record RegistrarDiagnosticoCommand(
         Guid OrdemServicoId,
         string Diagnostico);
}
