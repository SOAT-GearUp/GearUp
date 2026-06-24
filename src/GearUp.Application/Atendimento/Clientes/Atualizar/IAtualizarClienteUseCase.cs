namespace GearUp.Application.Atendimento.Clientes.Atualizar;

public interface IAtualizarClienteUseCase
{
    Task AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken);
}
