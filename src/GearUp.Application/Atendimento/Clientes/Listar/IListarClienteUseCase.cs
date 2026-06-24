namespace GearUp.Application.Atendimento.Clientes.Listar;

public interface IListarClienteUseCase
{
    Task<IReadOnlyList<ListarClienteResult>> ListarAsync(CancellationToken cancellationToken);
}
