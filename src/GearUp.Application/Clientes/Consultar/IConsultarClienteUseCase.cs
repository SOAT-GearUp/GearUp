namespace GearUp.Application.Clientes.Consultar
{
    public interface IConsultarClienteUseCase
    {
        Task<ConsultarClienteResult> ObterAsync(Guid id, CancellationToken cancellationToken);
    }
}
