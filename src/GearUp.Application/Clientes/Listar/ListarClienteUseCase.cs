using GearUp.Application.Clientes.Common.Interfaces;

namespace GearUp.Application.Clientes.Listar
{
    internal sealed class ListarClienteUseCase(IClienteRepository clienteRepository) : IListarClienteUseCase
    {
        public async Task<IReadOnlyList<ListarClienteResult>> ListarAsync(CancellationToken cancellationToken)
        {
            var clientes = await clienteRepository.ListarAsync(cancellationToken);

            return clientes.Select(c => new ListarClienteResult(
                Id: c.Id,
                Nome: c.Nome,
                Email: c.Email.ToString(),
                Telefone: c.Telefone.ToString()
            )).ToList();
        }
    }
}
