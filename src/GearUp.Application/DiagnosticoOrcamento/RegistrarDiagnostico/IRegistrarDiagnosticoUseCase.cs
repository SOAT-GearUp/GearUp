namespace GearUp.Application.DiagnosticoOrcamento.RegistrarDiagnostico;

public interface IRegistrarDiagnosticoUseCase
{
    Task RegistrarAsync(RegistrarDiagnosticoCommand command, CancellationToken ct);
}
