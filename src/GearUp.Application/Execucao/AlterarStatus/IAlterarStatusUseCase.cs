namespace GearUp.Application.Execucao.AlterarStatus;

public interface IAlterarStatusUseCase
{
    Task AlterarAsync(AlterarStatusCommand command, CancellationToken ct);
}
