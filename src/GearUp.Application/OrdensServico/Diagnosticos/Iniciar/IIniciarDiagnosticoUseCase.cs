namespace GearUp.Application.OrdensServico.Diagnosticos.Iniciar
{
    public interface IIniciarDiagnosticoUseCase
    {
        Task IniciarAsync(IniciarDiagnosticoCommand command, CancellationToken cancellationToken);
    }
}
