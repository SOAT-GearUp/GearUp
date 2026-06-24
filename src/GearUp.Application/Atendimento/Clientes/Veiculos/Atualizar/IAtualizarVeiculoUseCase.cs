namespace GearUp.Application.Atendimento.Clientes.Veiculos.Atualizar;

public interface IAtualizarVeiculoUseCase
{
    Task AtualizarVeiculoAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken);
}
