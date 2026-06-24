namespace GearUp.Application.Atendimento.Clientes.Excluir;

public interface IExcluirClienteUseCase
{
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken);
}
