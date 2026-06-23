namespace GearUp.Application.OrdensServico.Diagnosticos.Registrar
{
    public interface IRegistrarDiagnosticoUseCase
    {
        Task RegistrarAsync(RegistrarDiagnosticoCommand command, CancellationToken ct);
    }
}
