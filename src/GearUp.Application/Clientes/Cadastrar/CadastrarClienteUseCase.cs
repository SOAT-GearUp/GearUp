using GearUp.Application.Common;
using GearUp.Domain.Entities;

namespace GearUp.Application.Clientes.Cadastrar;

internal sealed class CadastrarClienteUseCase(
    IClienteRepository clienteRepository,
    IUnitOfWork unitOfWork)
    : ICadastrarClienteUseCase
{
    public async Task<CadastrarClienteResult> ExecutarAsync(
        CadastrarClienteCommand command,
        CancellationToken cancellationToken)
    {
        var cliente = Cliente.Criar(
            command.Nome,
            command.Documento,
            command.Email,
            command.Telefone);

        if (await clienteRepository.DocumentoExisteAsync(
                cliente.Documento,
                cancellationToken))
        {
            throw new ClienteDocumentoDuplicadoException(
                cliente.Documento.Numero);
        }

        await clienteRepository.AdicionarAsync(cliente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CadastrarClienteResult(cliente.Id);
    }
}
