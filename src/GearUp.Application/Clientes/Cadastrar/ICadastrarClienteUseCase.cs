namespace GearUp.Application.Clientes.Cadastrar;

public interface ICadastrarClienteUseCase
{
    Task<CadastrarClienteResult> ExecutarAsync(
        CadastrarClienteCommand command,
        CancellationToken cancellationToken);
}
