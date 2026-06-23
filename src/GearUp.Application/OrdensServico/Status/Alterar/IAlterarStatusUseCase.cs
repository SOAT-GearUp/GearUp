namespace GearUp.Application.OrdensServico.Status.Alterar
{
    public interface IAlterarStatusUseCase
    {
        Task AlterarAsync(AlterarStatusCommand command, CancellationToken ct);
    }
}
