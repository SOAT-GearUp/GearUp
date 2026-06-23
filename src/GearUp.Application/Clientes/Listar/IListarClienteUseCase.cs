namespace GearUp.Application.Clientes.Listar
{
    public interface IListarClienteUseCase
    {
        Task<IReadOnlyList<ListarClienteResult>> ListarAsync(CancellationToken cancellationToken);
    }
}
