using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Common;
using GearUp.Domain.Entities;
using GearUp.Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence;

public sealed class GearUpDbContext(
    DbContextOptions<GearUpDbContext> options,
    IDomainEventDispatcher? domainEventDispatcher = null)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Estoque> EstoqueItens => Set<Estoque>();
    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GearUpDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (domainEventDispatcher is not null)
        {
            var aggregates = ChangeTracker
                .Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count != 0)
                .ToList();

            var domainEvents = aggregates
                .SelectMany(aggregate => aggregate.DomainEvents)
                .ToList();

            aggregates.ForEach(aggregate => aggregate.LimparDomainEvents());

            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
