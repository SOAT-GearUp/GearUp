namespace GearUp.Application.Atendimento.Clientes.Consultar;

public interface IConsultarClienteUseCase
{
    Task<ConsultarClienteResult> ObterAsync(Guid id, CancellationToken cancellationToken);
}
