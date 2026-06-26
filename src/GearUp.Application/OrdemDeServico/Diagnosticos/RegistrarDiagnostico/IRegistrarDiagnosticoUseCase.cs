namespace GearUp.Application.OrdemDeServico.Diagnosticos.RegistrarDiagnostico;

public interface IRegistrarDiagnosticoUseCase
{
    Task RegistrarAsync(RegistrarDiagnosticoCommand command, CancellationToken ct);
}
