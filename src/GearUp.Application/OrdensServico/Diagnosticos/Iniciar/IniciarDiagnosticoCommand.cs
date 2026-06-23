namespace GearUp.Application.OrdensServico.Diagnosticos.Iniciar
{
    public sealed record IniciarDiagnosticoCommand(
        Guid OrdemServicoId,
        Guid MecanicoId);
}
