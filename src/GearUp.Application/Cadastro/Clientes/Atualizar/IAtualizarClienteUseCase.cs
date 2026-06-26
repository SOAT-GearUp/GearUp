namespace GearUp.Application.Cadastro.Clientes.Atualizar;

public interface IAtualizarClienteUseCase
{
    Task AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken);
}
