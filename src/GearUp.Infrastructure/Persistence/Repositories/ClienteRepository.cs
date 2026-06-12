using GearUp.Application.Clientes;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class ClienteRepository(GearUpDbContext dbContext)
    : IClienteRepository
{
    public Task<bool> DocumentoExisteAsync(
        Documento documento,
        CancellationToken cancellationToken) =>
        dbContext.Clientes
            .IgnoreQueryFilters()
            .AnyAsync(
                cliente => cliente.Documento == documento,
                cancellationToken);

    public async Task AdicionarAsync(
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        await dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }
}
