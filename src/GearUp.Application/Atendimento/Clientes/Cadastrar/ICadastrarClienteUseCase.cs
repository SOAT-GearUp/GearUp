namespace GearUp.Application.Atendimento.Clientes.Cadastrar;

public interface ICadastrarClienteUseCase
{
    Task<CadastrarClienteResult> CadastrarAsync(CadastrarClienteCommand command, CancellationToken cancellationToken);
}
