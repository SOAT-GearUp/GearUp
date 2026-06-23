namespace GearUp.Application.Clientes.Excluir
{
    public interface IExcluirClienteUseCase
    {
        Task ExcluirAsync(Guid id, CancellationToken cancellationToken);
    }
}
