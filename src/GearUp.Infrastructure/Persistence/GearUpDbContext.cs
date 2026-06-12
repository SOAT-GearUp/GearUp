using GearUp.Application.Common;
using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence;

public sealed class GearUpDbContext(DbContextOptions<GearUpDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GearUpDbContext).Assembly);
    }
}
