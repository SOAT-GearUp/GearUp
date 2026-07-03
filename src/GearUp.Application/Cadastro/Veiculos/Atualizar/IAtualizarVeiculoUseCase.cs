namespace GearUp.Application.Cadastro.Veiculos.Atualizar;

public interface IAtualizarVeiculoUseCase
{
    Task AtualizarVeiculoAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken);
}
