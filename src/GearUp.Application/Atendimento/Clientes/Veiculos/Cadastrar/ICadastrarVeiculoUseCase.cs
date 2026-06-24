namespace GearUp.Application.Atendimento.Clientes.Veiculos.Cadastrar;

public interface ICadastrarVeiculoUseCase
{
    Task<CadastrarVeiculoResult> CadastrarVeiculoAsync(CadastrarVeiculoCommand command, CancellationToken cancellationToken);
}
