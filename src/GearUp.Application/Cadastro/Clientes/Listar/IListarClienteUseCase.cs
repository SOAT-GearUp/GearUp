namespace GearUp.Application.Cadastro.Clientes.Listar;

public interface IListarClienteUseCase
{
    Task<IReadOnlyList<ListarClienteResult>> ListarAsync(CancellationToken cancellationToken);
}
