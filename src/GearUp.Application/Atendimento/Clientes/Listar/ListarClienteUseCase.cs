using GearUp.Application.Atendimento.Clientes.Common.Interfaces;

namespace GearUp.Application.Atendimento.Clientes.Listar;

internal sealed class ListarClienteUseCase(IClienteRepository clienteRepository) : IListarClienteUseCase
{
    public async Task<IReadOnlyList<ListarClienteResult>> ListarAsync(CancellationToken cancellationToken)
    {
        var clientes = await clienteRepository.ListarAsync(cancellationToken);

        return clientes.Select(c => new ListarClienteResult(c.Id, c.Nome, c.Email.ToString(), c.Telefone.ToString())).ToList();
    }
}
