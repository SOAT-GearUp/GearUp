using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Decidir;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;
using EstoqueAggregate = GearUp.Domain.Entities.Estoque;

namespace GearUp.Application.UnitTests.OrdemDeServico.Orcamentos.Decidir;

public sealed class DecidirOrcamentoUseCaseTests
{
    [Fact]
    public async Task DecidirAsync_AprovandoOrcamentoSemItensDeEstoque_DeveIrParaAguardandoExecucao()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var orcamento = CriarOrcamentoSemItensDeEstoque(os.Id);
        var useCase = CriarUseCase(os, orcamento, estoque: [], out var unitOfWork);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: true);

        await useCase.DecidirAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrcamento.Aprovado, orcamento.Status);
        Assert.Equal(StatusOrdemServico.AguardandoExecucao, os.Status);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task DecidirAsync_AprovandoOrcamentoComEstoqueSuficiente_DeveIrParaAguardandoExecucao()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var estoque = EstoqueAggregate.Criar("Correia", TipoItemEstoque.Peca, 150m, quantidadeInicial: 5);
        var orcamento = CriarOrcamentoComPeca(os.Id, estoque.Id);
        var useCase = CriarUseCase(os, orcamento, estoque: [estoque], out var unitOfWork);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: true);

        await useCase.DecidirAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.AguardandoExecucao, os.Status);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task DecidirAsync_AprovandoOrcamentoComEstoqueInsuficiente_DeveIrParaAguardandoPecasInsumos()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var estoque = EstoqueAggregate.Criar("Correia", TipoItemEstoque.Peca, 150m, quantidadeInicial: 1);
        var orcamento = CriarOrcamentoComPeca(os.Id, estoque.Id, quantidade: 3);
        var useCase = CriarUseCase(os, orcamento, estoque: [estoque], out _);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: true);

        await useCase.DecidirAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.AguardandoPecasInsumos, os.Status);
    }

    [Fact]
    public async Task DecidirAsync_AprovandoOrcamentoComItemDeEstoqueInexistente_DeveIrParaAguardandoPecasInsumos()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var orcamento = CriarOrcamentoComPeca(os.Id, Guid.NewGuid());
        var useCase = CriarUseCase(os, orcamento, estoque: [], out _);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: true);

        await useCase.DecidirAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.AguardandoPecasInsumos, os.Status);
    }

    [Fact]
    public async Task DecidirAsync_ReprovandoOrcamentoPendente_DeveRejeitarESalvar()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var orcamento = CriarOrcamentoComPeca(os.Id, Guid.NewGuid());
        var useCase = CriarUseCase(os, orcamento, estoque: [], out var unitOfWork);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: false);

        await useCase.DecidirAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrcamento.Rejeitado, orcamento.Status);
        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, os.Status);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task DecidirAsync_ComOrcamentoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new DecidirOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, new EstoqueRepositoryFake([]), unitOfWork);
        var command = new DecidirOrcamentoCommand(os.Id, Guid.NewGuid(), Aprovado: true);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.DecidirAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task DecidirAsync_ComOrdemServicoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var orcamento = CriarOrcamentoComPeca(Guid.NewGuid(), Guid.NewGuid());
        var ordemServicoRepository = new OrdemServicoRepositoryFake(null);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new DecidirOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, new EstoqueRepositoryFake([]), unitOfWork);
        var command = new DecidirOrcamentoCommand(Guid.NewGuid(), orcamento.Id, Aprovado: true);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.DecidirAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task DecidirAsync_ComOrcamentoJaDecidido_DeveLancarRegraNegocio()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var orcamento = CriarOrcamentoComPeca(os.Id, Guid.NewGuid());
        orcamento.Decidir(aprovado: true);
        var useCase = CriarUseCase(os, orcamento, estoque: [], out var unitOfWork);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: true);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.DecidirAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task DecidirAsync_ComOrdemServicoEmStatusInvalido_DeveLancarRegraNegocio()
    {
        var os = CriarOrdemServicoAguardandoOrcamento();
        var orcamento = CriarOrcamentoComPeca(os.Id, Guid.NewGuid());
        var useCase = CriarUseCase(os, orcamento, estoque: [], out var unitOfWork);
        var command = new DecidirOrcamentoCommand(os.Id, orcamento.Id, Aprovado: true);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.DecidirAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static DecidirOrcamentoUseCase CriarUseCase(
        OrdemServico os,
        Orcamento orcamento,
        IReadOnlyList<EstoqueAggregate> estoque,
        out UnitOfWorkFake unitOfWork)
    {
        unitOfWork = new UnitOfWorkFake();
        return new DecidirOrcamentoUseCase(
            new OrdemServicoRepositoryFake(os),
            new OrcamentoRepositoryFake(orcamento),
            new EstoqueRepositoryFake(estoque),
            unitOfWork);
    }

    private static Orcamento CriarOrcamentoComPeca(Guid ordemServicoId, Guid estoqueItemId, decimal quantidade = 1) =>
        Orcamento.Criar(ordemServicoId, 1,
        [
            NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, "Correia", quantidade, 150m, estoqueItemId),
        ]);

    private static Orcamento CriarOrcamentoSemItensDeEstoque(Guid ordemServicoId) =>
        Orcamento.Criar(ordemServicoId, 1,
        [
            NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Troca de óleo", 1, 120m, null),
        ]);

    private static OrdemServico CriarOrdemServicoAguardandoOrcamento()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Solicitação", PrioridadeOrdemServico.Normal, null);
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnóstico concluído");
        return os;
    }

    private static OrdemServico CriarOrdemServicoAguardandoAprovacao()
    {
        var os = CriarOrdemServicoAguardandoOrcamento();
        os.AguardarAprovacao(Guid.NewGuid(), 1);
        return os;
    }

    private sealed class OrdemServicoRepositoryFake(OrdemServico? ordemServico) : IOrdemServicoRepository
    {
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(ordemServico);
    }

    private sealed class OrcamentoRepositoryFake(Orcamento? orcamento) : IOrcamentoRepository
    {
        public Task AdicionarAsync(Orcamento orcamento, CancellationToken ct) => Task.CompletedTask;

        public Task<Orcamento?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(orcamento);

        public Task<int> ContarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) =>
            Task.FromResult(0);
    }

    private sealed class EstoqueRepositoryFake(IReadOnlyList<EstoqueAggregate> itens) : IEstoqueRepository
    {
        public Task AdicionarAsync(EstoqueAggregate item, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<EstoqueAggregate?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(itens.SingleOrDefault(i => i.Id == id));

        public Task<IReadOnlyList<EstoqueAggregate>> ListarAsync(CancellationToken cancellationToken) =>
            Task.FromResult(itens);
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
