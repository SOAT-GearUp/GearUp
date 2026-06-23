using GearUp.Application.Clientes.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class ClienteRepository(GearUpDbContext dbContext)
    : IClienteRepository
{
    public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .Include(cliente => cliente.Veiculos)
            .SingleOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Clientes
            .AsNoTracking()
            .Include(cliente => cliente.Veiculos)
            .OrderBy(cliente => cliente.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Veiculo?> ObterVeiculoAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Veiculos.SingleOrDefaultAsync(veiculo => veiculo.Id == id, cancellationToken);

    public Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken cancellationToken)
    {
        var normalizada = placa.Replace("-", string.Empty).Trim().ToUpperInvariant();
        
        return dbContext.Veiculos
            .IgnoreQueryFilters()
            .AnyAsync(v => v.Placa == normalizada && (!ignorarId.HasValue || v.Id != ignorarId.Value), cancellationToken);
    }

    public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .IgnoreQueryFilters()
            .AnyAsync(
                cliente => cliente.Documento == documento,
                cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        await dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }
}
