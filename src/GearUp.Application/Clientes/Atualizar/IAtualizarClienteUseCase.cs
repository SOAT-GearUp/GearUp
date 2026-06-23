namespace GearUp.Application.Clientes.Atualizar
{
    public interface IAtualizarClienteUseCase
    {
        Task AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken);
    }
}
