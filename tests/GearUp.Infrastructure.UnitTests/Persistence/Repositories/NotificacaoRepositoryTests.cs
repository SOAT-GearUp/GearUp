using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class NotificacaoRepositoryTests
{
    [Fact]
    public async Task ListarAsync_DeveFiltrarPorDestinatarioECliente()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<INotificacaoRepository>();
        var clienteId = Guid.NewGuid();
        var notificacaoCliente = Notificacao.Criar(Guid.NewGuid(), clienteId, DestinatarioNotificacao.Cliente, "Mensagem cliente");
        var notificacaoAtendente = Notificacao.Criar(Guid.NewGuid(), clienteId, DestinatarioNotificacao.Atendente, "Mensagem atendente");
        var notificacaoOutroCliente = Notificacao.Criar(Guid.NewGuid(), Guid.NewGuid(), DestinatarioNotificacao.Cliente, "Outro cliente");

        await repository.AdicionarAsync(notificacaoCliente, CancellationToken.None);
        await repository.AdicionarAsync(notificacaoAtendente, CancellationToken.None);
        await repository.AdicionarAsync(notificacaoOutroCliente, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var notificacoes = await repository.ListarAsync(DestinatarioNotificacao.Cliente, clienteId, CancellationToken.None);

        var notificacao = Assert.Single(notificacoes);
        Assert.Equal(notificacaoCliente.Id, notificacao.Id);
        Assert.Equal("Mensagem cliente", notificacao.Mensagem);
    }
}
