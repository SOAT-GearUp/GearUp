using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.Clientes;

public interface IClienteRepository
{
    Task<bool> DocumentoExisteAsync(
        Documento documento,
        CancellationToken cancellationToken);

    Task AdicionarAsync(
        Cliente cliente,
        CancellationToken cancellationToken);
}
