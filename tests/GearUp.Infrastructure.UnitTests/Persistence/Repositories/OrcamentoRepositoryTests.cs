using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;
using GearUp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

using IAtendimentoOrcamentoRepository = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrcamentoRepository;
using IDiagnosticoOrcamentoRepository = GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces.IOrcamentoRepository;
using IExecucaoOrcamentoRepository = GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces.IOrcamentoRepository;

namespace GearUp.Infrastructure.UnitTests.Persistence.Repositories;

public sealed class OrcamentoRepositoryTests
{
    [Fact]
    public async Task RepositoriosDeOrcamento_DevePersistirListarContarEObterAprovado()
    {
        await using var factory = new InMemoryDbContextFactory();
        using var scope = factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GearUpDbContext>();
        var diagnosticoRepository = scope.ServiceProvider.GetRequiredService<IDiagnosticoOrcamentoRepository>();
        var atendimentoRepository = scope.ServiceProvider.GetRequiredService<IAtendimentoOrcamentoRepository>();
        var execucaoRepository = scope.ServiceProvider.GetRequiredService<IExecucaoOrcamentoRepository>();
        var ordemServicoId = Guid.NewGuid();
        var orcamentoV2 = CriarOrcamento(ordemServicoId, 2);
        var orcamentoV1 = CriarOrcamento(ordemServicoId, 1);
        var outroOrcamento = CriarOrcamento(Guid.NewGuid(), 1);
        orcamentoV2.Decidir(aprovado: true);

        await diagnosticoRepository.AdicionarAsync(orcamentoV2, CancellationToken.None);
        await diagnosticoRepository.AdicionarAsync(orcamentoV1, CancellationToken.None);
        await diagnosticoRepository.AdicionarAsync(outroOrcamento, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var encontrado = await diagnosticoRepository.ObterAsync(orcamentoV1.Id, CancellationToken.None);
        var listados = await atendimentoRepository.ListarPorOrdemServicoAsync(ordemServicoId, CancellationToken.None);
        var quantidade = await diagnosticoRepository.ContarPorOrdemServicoAsync(ordemServicoId, CancellationToken.None);
        var aprovado = await execucaoRepository.ObterAprovadoPorOrdemServicoAsync(ordemServicoId, CancellationToken.None);

        Assert.NotNull(encontrado);
        Assert.Single(encontrado.Itens);
        Assert.Equal(2, quantidade);
        Assert.Collection(
            listados,
            orcamento => Assert.Equal(orcamentoV1.Id, orcamento.Id),
            orcamento => Assert.Equal(orcamentoV2.Id, orcamento.Id));
        Assert.NotNull(aprovado);
        Assert.Equal(orcamentoV2.Id, aprovado.Id);
    }

    private static Orcamento CriarOrcamento(Guid ordemServicoId, int versao)
    {
        return Orcamento.Criar(
            ordemServicoId,
            versao,
            [NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Alinhamento", 1, 100m, null)]);
    }
}
