using GearUp.Application.Atendimento.Clientes.Veiculos.Common.Interfaces;
using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class VeiculoRepository(GearUpDbContext db) : IVeiculoRepository
{
    public Task<Veiculo?> ObterAsync(Guid id, CancellationToken ct) =>
        db.Veiculos.SingleOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct) =>
        await db.Veiculos.AsNoTracking().Where(v => v.ClienteId == clienteId).ToListAsync(ct);

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken ct) =>
        await db.Veiculos.AddAsync(veiculo, ct);

    public Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken ct)
    {
        var normalizada = placa.Replace("-", string.Empty).Trim().ToUpperInvariant();
        return db.Veiculos
            .IgnoreQueryFilters()
            .AnyAsync(v => v.Placa == normalizada && (!ignorarId.HasValue || v.Id != ignorarId.Value), ct);
    }
}
