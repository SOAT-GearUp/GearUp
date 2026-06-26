namespace GearUp.Application.Cadastro.Clientes.Veiculos.Cadastrar;

public interface ICadastrarVeiculoUseCase
{
    Task<CadastrarVeiculoResult> CadastrarVeiculoAsync(CadastrarVeiculoCommand command, CancellationToken cancellationToken);
}
