using GearUp.Domain.Entities;

namespace GearUp.Application.Cadastro.Veiculos.Common.Interfaces;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken ct);
    Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken ct);
}
