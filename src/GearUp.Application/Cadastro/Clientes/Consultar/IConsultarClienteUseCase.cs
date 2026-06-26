namespace GearUp.Application.Cadastro.Clientes.Consultar;

public interface IConsultarClienteUseCase
{
    Task<ConsultarClienteResult> ObterAsync(Guid id, CancellationToken cancellationToken);
}
