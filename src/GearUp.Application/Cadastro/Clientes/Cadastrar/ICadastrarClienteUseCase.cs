namespace GearUp.Application.Cadastro.Clientes.Cadastrar;

public interface ICadastrarClienteUseCase
{
    Task<CadastrarClienteResult> CadastrarAsync(CadastrarClienteCommand command, CancellationToken cancellationToken);
}
