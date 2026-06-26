using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

using IOrdemServicoRepository = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrdemServicoRepository;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class OrdemServicoRepositoryTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirOrdemECarregarHistorico()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrdemServicoRepository>();
        var ordem = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Troca de oleo", PrioridadeOrdemServico.Normal, null);

        await repository.AdicionarAsync(ordem, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var encontrada = await repository.ObterAsync(ordem.Id, CancellationToken.None);

        Assert.NotNull(encontrada);
        Assert.Equal(ordem.Id, encontrada.Id);
        Assert.Contains(encontrada.Historico, historico => historico.Tipo == "OS_CRIADA");
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarAndamentoEClienteEOrdenarPorPrioridade()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrdemServicoRepository>();
        var clienteId = Guid.NewGuid();
        var urgente = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico urgente", PrioridadeOrdemServico.Urgente, null);
        var normal = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico normal", PrioridadeOrdemServico.Normal, null);
        var cancelada = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico cancelado", PrioridadeOrdemServico.Alta, null);
        var outroCliente = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Outro cliente", PrioridadeOrdemServico.Urgente, null);
        cancelada.AlterarStatus(StatusOrdemServico.Cancelada);

        await dbContext.OrdensServico.AddRangeAsync(normal, cancelada, urgente, outroCliente);
        await dbContext.SaveChangesAsync();

        var ordens = await repository.ListarAsync(somenteEmAndamento: true, clienteId, CancellationToken.None);

        Assert.Collection(
            ordens,
            ordem => Assert.Equal(urgente.Id, ordem.Id),
            ordem => Assert.Equal(normal.Id, ordem.Id));
    }
}
