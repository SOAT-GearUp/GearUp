using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.Atendimento.Clientes.Common.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken);
    Task<Veiculo?> ObterVeiculoAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken cancellationToken);
    Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken);
}
