namespace GearUp.Application.Cadastro.Clientes.Veiculos.Atualizar;

public interface IAtualizarVeiculoUseCase
{
    Task AtualizarVeiculoAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken);
}
