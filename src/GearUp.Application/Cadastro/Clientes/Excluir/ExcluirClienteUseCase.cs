using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Common.Interfaces;

namespace GearUp.Application.Cadastro.Clientes.Excluir;

internal sealed class ExcluirClienteUseCase(IClienteRepository clienteRepository, IUnitOfWork unitOfWork) : IExcluirClienteUseCase
{
    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await clienteRepository.ObterAsync(id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

        cliente.Excluir();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
