using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Execucao.AlterarStatus;
using GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.UnitTests.OrdemDeServico.Execucao.AlterarStatus;

public sealed class AlterarStatusUseCaseTests
{
    [Fact]
    public async Task AlterarAsync_ComOrdemServicoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var ordemServicoRepository = new OrdemServicoRepositoryFake(ordemServico: null);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AlterarStatusUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new AlterarStatusCommand(Guid.NewGuid(), StatusOrdemServico.Cancelada);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AlterarAsync(command, CancellationToken.None));

        Assert.Equal("OS_NAO_ENCONTRADA", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AlterarAsync_ParaEmExecucaoSemOrcamentoAprovado_DeveLancarRecursoNaoEncontrado()
    {
        var os = CriarOrdemServicoAguardandoExecucao();
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AlterarStatusUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new AlterarStatusCommand(os.Id, StatusOrdemServico.EmExecucao);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AlterarAsync(command, CancellationToken.None));

        Assert.Equal("ORCAMENTO_APROVADO_NAO_ENCONTRADO", ex.Codigo);
        Assert.Equal(os.Id, orcamentoRepository.OrdemServicoIdConsultado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AlterarAsync_ParaEmExecucaoComOrcamentoAprovado_DeveIniciarExecucaoESalvar()
    {
        var os = CriarOrdemServicoAguardandoExecucao();
        var orcamento = CriarOrcamentoComItens(os.Id);
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AlterarStatusUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new AlterarStatusCommand(os.Id, StatusOrdemServico.EmExecucao);

        await useCase.AlterarAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        Assert.NotNull(os.IniciadaEm);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AlterarAsync_ParaStatusDiferenteDeEmExecucao_DeveAlterarStatusESalvar()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Ruído nos freios", PrioridadeOrdemServico.Normal, null);
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AlterarStatusUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new AlterarStatusCommand(os.Id, StatusOrdemServico.Cancelada);

        await useCase.AlterarAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Cancelada, os.Status);
        Assert.Null(orcamentoRepository.OrdemServicoIdConsultado);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    private static OrdemServico CriarOrdemServicoAguardandoExecucao()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Ruído nos freios", PrioridadeOrdemServico.Normal, null);
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Pastilhas gastas");
        var orcamentoId = Guid.NewGuid();
        os.AguardarAprovacao(orcamentoId, 1);
        os.ReceberDecisaoOrcamento(orcamentoId, true, estoqueDisponivelParaExecucao: true);
        return os;
    }

    private static Orcamento CriarOrcamentoComItens(Guid ordemServicoId)
    {
        var itens = new[]
        {
            NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, "Pastilha de freio", 2, 80m, Guid.NewGuid()),
            NovoItemOrcamento.Criar(TipoItemOrcamento.MaoDeObra, "Troca de pastilhas", 1, 120m, null)
        };

        var orcamento = Orcamento.Criar(ordemServicoId, 1, itens);
        orcamento.Decidir(true);
        return orcamento;
    }

    private sealed class OrdemServicoRepositoryFake(OrdemServico? ordemServico) : IOrdemServicoRepository
    {
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(ordemServico);

        public Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<OrdemServico>>([]);
    }

    private sealed class OrcamentoRepositoryFake(Orcamento? orcamento) : IOrcamentoRepository
    {
        public Guid? OrdemServicoIdConsultado { get; private set; }

        public Task<Orcamento?> ObterAprovadoPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct)
        {
            OrdemServicoIdConsultado = ordemServicoId;
            return Task.FromResult(orcamento);
        }
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveChangesChamadas { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesChamadas++;
            return Task.FromResult(1);
        }
    }
}
