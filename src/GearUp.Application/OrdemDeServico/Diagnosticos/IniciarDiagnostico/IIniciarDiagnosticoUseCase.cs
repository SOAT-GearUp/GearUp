namespace GearUp.Application.OrdemDeServico.Diagnosticos.IniciarDiagnostico;

public interface IIniciarDiagnosticoUseCase
{
    Task IniciarAsync(IniciarDiagnosticoCommand command, CancellationToken ct);
}
