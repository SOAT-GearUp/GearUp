using GearUp.Application.Atendimento.Clientes.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories;

internal sealed class ClienteRepository(GearUpDbContext dbContext) : IClienteRepository
{
    public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .SingleOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken)
    {
        return dbContext.Clientes
            .IgnoreQueryFilters()
            .AnyAsync(cliente => cliente.Documento == documento, cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        await dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }
}
