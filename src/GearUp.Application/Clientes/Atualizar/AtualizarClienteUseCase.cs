using GearUp.Application.Clientes.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.Clientes.Atualizar
{
    internal sealed class AtualizarClienteUseCase(IClienteRepository clienteRepository, IUnitOfWork unitOfWork) : IAtualizarClienteUseCase
    {
        public async Task AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken)
        {
            var cliente = await clienteRepository.ObterAsync(command.Id, cancellationToken)
                ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

            cliente.Atualizar(command.Nome, command.Email, command.Telefone);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
