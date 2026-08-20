using GearUp.Domain.Entities;
using GearUp.Domain.DomainEvents.Execucao;
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
    public async Task ListarAsync_DeveFiltrarAndamentoEClienteEOrdenarPorStatus()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrdemServicoRepository>();
        var clienteId = Guid.NewGuid();
        var recebida = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico recebido", PrioridadeOrdemServico.Urgente, null);
        var emDiagnostico = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico em diagnostico", PrioridadeOrdemServico.Normal, null);
        var aguardandoAprovacao = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico aguardando aprovacao", PrioridadeOrdemServico.Normal, null);
        var emExecucao = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico em execucao", PrioridadeOrdemServico.Normal, null);
        var finalizada = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico finalizado", PrioridadeOrdemServico.Alta, null);
        var entregue = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico entregue", PrioridadeOrdemServico.Alta, null);
        var cancelada = OrdemServico.Criar(clienteId, Guid.NewGuid(), "Servico cancelado", PrioridadeOrdemServico.Alta, null);
        var outroCliente = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Outro cliente", PrioridadeOrdemServico.Urgente, null);

        emDiagnostico.IniciarDiagnostico(Guid.NewGuid());
        aguardandoAprovacao.AguardarAprovacao(Guid.NewGuid(), 1);
        emExecucao.AguardarAprovacao(Guid.NewGuid(), 1);
        emExecucao.ReceberDecisaoOrcamento(Guid.NewGuid(), aprovado: true, estoqueDisponivelParaExecucao: true);
        emExecucao.IniciarExecucao([]);
        finalizada.AguardarAprovacao(Guid.NewGuid(), 1);
        finalizada.ReceberDecisaoOrcamento(Guid.NewGuid(), aprovado: true, estoqueDisponivelParaExecucao: true);
        finalizada.IniciarExecucao([]);
        finalizada.AlterarStatus(StatusOrdemServico.Finalizada);
        entregue.AguardarAprovacao(Guid.NewGuid(), 1);
        entregue.ReceberDecisaoOrcamento(Guid.NewGuid(), aprovado: true, estoqueDisponivelParaExecucao: true);
        entregue.IniciarExecucao([]);
        entregue.AlterarStatus(StatusOrdemServico.Finalizada);
        entregue.AlterarStatus(StatusOrdemServico.Entregue);
        cancelada.AlterarStatus(StatusOrdemServico.Cancelada);

        await dbContext.OrdensServico.AddRangeAsync(recebida, cancelada, emDiagnostico, finalizada, aguardandoAprovacao, entregue, emExecucao, outroCliente);
        await dbContext.SaveChangesAsync();

        var ordens = await repository.ListarAsync(somenteEmAndamento: true, clienteId, CancellationToken.None);

        Assert.Collection(
            ordens,
            ordem => Assert.Equal(emExecucao.Id, ordem.Id),
            ordem => Assert.Equal(aguardandoAprovacao.Id, ordem.Id),
            ordem => Assert.Equal(emDiagnostico.Id, ordem.Id),
            ordem => Assert.Equal(recebida.Id, ordem.Id));
    }
}
