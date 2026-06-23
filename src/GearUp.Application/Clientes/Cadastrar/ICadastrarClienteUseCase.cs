namespace GearUp.Application.Clientes.Cadastrar;

public interface ICadastrarClienteUseCase
{
    Task<CadastrarClienteResult> CadastrarAsync(
        CadastrarClienteCommand command,
        CancellationToken cancellationToken);
}
