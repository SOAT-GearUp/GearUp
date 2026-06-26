using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class ClienteRepositoryTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirClienteEPermitirConsultaPorId()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var cliente = Cliente.Criar("Jose Silva", "529.982.247-25", "jose@email.com", "11999999999");

        await repository.AdicionarAsync(cliente, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var encontrado = await repository.ObterAsync(cliente.Id, CancellationToken.None);

        Assert.NotNull(encontrado);
        Assert.Equal("Jose Silva", encontrado.Nome);
        Assert.Equal("52998224725", encontrado.Documento.Numero);
    }

    [Fact]
    public async Task ListarAsync_DeveIgnorarClienteExcluidoEOrdenarPorNome()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var ativoA = Cliente.Criar("Ana Silva", "529.982.247-25", "ana@email.com", "11999999999");
        var ativoB = Cliente.Criar("Bruno Silva", "111.444.777-35", "bruno@email.com", "11988888888");
        var excluido = Cliente.Criar("Carlos Silva", "390.533.447-05", "carlos@email.com", "11977777777");
        excluido.Excluir();

        await dbContext.Clientes.AddRangeAsync(ativoB, excluido, ativoA);
        await dbContext.SaveChangesAsync();

        var clientes = await repository.ListarAsync(CancellationToken.None);

        Assert.Collection(
            clientes,
            cliente => Assert.Equal(ativoA.Id, cliente.Id),
            cliente => Assert.Equal(ativoB.Id, cliente.Id));
    }

    [Fact]
    public async Task DocumentoExisteAsync_DeveConsiderarClienteExcluido()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var cliente = Cliente.Criar("Jose Silva", "529.982.247-25", "jose@email.com", "11999999999");
        cliente.Excluir();

        await dbContext.Clientes.AddAsync(cliente);
        await dbContext.SaveChangesAsync();

        var existe = await repository.DocumentoExisteAsync(Documento.Criar("529.982.247-25"), CancellationToken.None);

        Assert.True(existe);
    }
}
