namespace GearUp.Application.DiagnosticoOrcamento.IniciarDiagnostico;

public interface IIniciarDiagnosticoUseCase
{
    Task IniciarAsync(IniciarDiagnosticoCommand command, CancellationToken ct);
}
