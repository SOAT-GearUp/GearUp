namespace GearUp.Application.Cadastro.Clientes.Excluir;

public interface IExcluirClienteUseCase
{
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken);
}
