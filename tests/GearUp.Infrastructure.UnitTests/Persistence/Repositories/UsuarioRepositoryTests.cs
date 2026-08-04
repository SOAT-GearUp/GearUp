using GearUp.Application.Autenticacao.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class UsuarioRepositoryTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirUsuarioEPermitirConsultaPorNome()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var usuario = Usuario.Criar("Atendente", "hash", PerfilUsuario.Atendente);

        await repository.AdicionarAsync(usuario, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var existe = await repository.ExisteAsync("atendente", CancellationToken.None);
        var encontrado = await repository.ObterPorNomeAsync("atendente", CancellationToken.None);

        Assert.True(existe);
        Assert.NotNull(encontrado);
        Assert.Equal(usuario.Id, encontrado.Id);
        Assert.Equal("atendente", encontrado.NomeUsuario);
    }
}
