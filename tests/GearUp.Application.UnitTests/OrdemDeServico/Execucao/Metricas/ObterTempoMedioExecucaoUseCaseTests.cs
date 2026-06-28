using GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Execucao.Metricas;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.OrdemDeServico.Execucao.Metricas;

public sealed class ObterTempoMedioExecucaoUseCaseTests
{
    [Fact]
    public async Task ObterTempoMedioExecucaoAsync_SemOrdensServico_DeveRetornarNulo()
    {
        var repository = new OrdemServicoRepositoryFake([]);
        var useCase = new ObterTempoMedioExecucaoUseCase(repository);

        var result = await useCase.ObterTempoMedioExecucaoAsync(CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObterTempoMedioExecucaoAsync_SemOrdensConcluidas_DeveRetornarNulo()
    {
        var naoIniciada = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Serviço", PrioridadeOrdemServico.Normal, null);
        var repository = new OrdemServicoRepositoryFake([naoIniciada]);
        var useCase = new ObterTempoMedioExecucaoUseCase(repository);

        var result = await useCase.ObterTempoMedioExecucaoAsync(CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObterTempoMedioExecucaoAsync_ComOrdensConcluidas_DeveRetornarTempoMedio()
    {
        var concluida = CriarOrdemServicoFinalizada();
        var emExecucao = CriarOrdemServicoAguardandoExecucao();
        emExecucao.IniciarExecucao([]); // tem IniciadaEm, mas não FinalizadaEm -> ignorada
        var repository = new OrdemServicoRepositoryFake([concluida, emExecucao]);
        var useCase = new ObterTempoMedioExecucaoUseCase(repository);

        var result = await useCase.ObterTempoMedioExecucaoAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result!.TempoMedio);
        Assert.True(result.TempoMedio >= TimeSpan.Zero);
    }

    private static OrdemServico CriarOrdemServicoAguardandoExecucao()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Serviço", PrioridadeOrdemServico.Normal, null);
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnóstico");
        var orcamentoId = Guid.NewGuid();
        os.AguardarAprovacao(orcamentoId, 1);
        os.ReceberDecisaoOrcamento(orcamentoId, true, estoqueDisponivelParaExecucao: true);
        return os;
    }

    private static OrdemServico CriarOrdemServicoFinalizada()
    {
        var os = CriarOrdemServicoAguardandoExecucao();
        os.IniciarExecucao([]);
        os.AlterarStatus(StatusOrdemServico.Finalizada);
        return os;
    }

    private sealed class OrdemServicoRepositoryFake(IReadOnlyList<OrdemServico> ordens) : IOrdemServicoRepository
    {
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<OrdemServico?>(null);

        public Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct) =>
            Task.FromResult(ordens);
    }
}
