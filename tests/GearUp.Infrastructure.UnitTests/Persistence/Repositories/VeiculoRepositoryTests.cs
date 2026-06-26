using GearUp.Application.Cadastro.Clientes.Veiculos.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class VeiculoRepositoryTests
{
    [Fact]
    public async Task PlacaExisteAsync_DeveNormalizarPlacaEConsiderarExcluidos()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IVeiculoRepository>();
        var cliente = Cliente.Criar("Jose Silva", "529.982.247-25", "jose@email.com", "11999999999");
        var veiculo = Veiculo.Criar(cliente.Id, "abc-1d23", "Fiat", "Uno", 2020);
        veiculo.Excluir();

        await dbContext.Clientes.AddAsync(cliente);
        await dbContext.Veiculos.AddAsync(veiculo);
        await dbContext.SaveChangesAsync();

        var existe = await repository.PlacaExisteAsync("ABC-1D23", null, CancellationToken.None);

        Assert.True(existe);
    }

    [Fact]
    public async Task ListarPorClienteAsync_DeveRetornarSomenteVeiculosAtivosDoCliente()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IVeiculoRepository>();
        var cliente = Cliente.Criar("Jose Silva", "529.982.247-25", "jose@email.com", "11999999999");
        var outroCliente = Cliente.Criar("Ana Silva", "111.444.777-35", "ana@email.com", "11988888888");
        var veiculoAtivo = Veiculo.Criar(cliente.Id, "ABC1D23", "Fiat", "Uno", 2020);
        var veiculoExcluido = Veiculo.Criar(cliente.Id, "DEF4G56", "Ford", "Ka", 2019);
        var veiculoOutroCliente = Veiculo.Criar(outroCliente.Id, "GHI7J89", "VW", "Gol", 2021);
        veiculoExcluido.Excluir();

        await dbContext.Clientes.AddRangeAsync(cliente, outroCliente);
        await dbContext.Veiculos.AddRangeAsync(veiculoAtivo, veiculoExcluido, veiculoOutroCliente);
        await dbContext.SaveChangesAsync();

        var veiculos = await repository.ListarPorClienteAsync(cliente.Id, CancellationToken.None);

        var veiculo = Assert.Single(veiculos);
        Assert.Equal(veiculoAtivo.Id, veiculo.Id);
    }
}
