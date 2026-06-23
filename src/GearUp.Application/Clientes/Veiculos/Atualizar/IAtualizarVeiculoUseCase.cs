namespace GearUp.Application.Clientes.Veiculos.Atualizar
{
    public interface IAtualizarVeiculoUseCase
    {
        Task AtualizarVeiculoAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken);
    }
}
