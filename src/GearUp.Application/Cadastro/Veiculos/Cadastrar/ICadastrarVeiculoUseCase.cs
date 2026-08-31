namespace GearUp.Application.Cadastro.Veiculos.Cadastrar;

public interface ICadastrarVeiculoUseCase
{
    Task<CadastrarVeiculoResult> CadastrarVeiculoAsync(CadastrarVeiculoCommand command, CancellationToken cancellationToken);
}
