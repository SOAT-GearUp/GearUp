using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Criar;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.OrdemDeServico.Orcamentos.Criar;

public sealed class CriarOrcamentoUseCaseTests
{
    [Fact]
    public async Task CriarAsync_ComOrdemServicoAguardandoOrcamento_DeveCriarOrcamentoESalvar()
    {
        var os = CriarOrdemServicoAguardandoOrcamento();
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(quantidadeExistente: 2);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new CriarOrcamentoCommand(os.Id,
        [
            new CriarItemOrcamentoCommand(TipoItemOrcamento.Peca, "Correia", 1, 150m, Guid.NewGuid()),
            new CriarItemOrcamentoCommand(TipoItemOrcamento.MaoDeObra, "Troca", 2, 100m, null),
        ]);

        var result = await useCase.CriarAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(3, result.Versao);
        Assert.Equal(350m, result.ValorTotal);
        Assert.NotNull(orcamentoRepository.OrcamentoAdicionado);
        Assert.Equal(result.Id, orcamentoRepository.OrcamentoAdicionado.Id);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComOrdemServicoRecebida_DeveCriarOrcamentoESalvar()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Troca de oleo", PrioridadeOrdemServico.Normal, null);
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(quantidadeExistente: 0);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new CriarOrcamentoCommand(os.Id,
        [
            new CriarItemOrcamentoCommand(TipoItemOrcamento.Servico, "Troca de oleo", 1, 120m, null),
        ]);

        var result = await useCase.CriarAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(1, result.Versao);
        Assert.Equal(120m, result.ValorTotal);
        Assert.NotNull(orcamentoRepository.OrcamentoAdicionado);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComOrdemServicoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var ordemServicoRepository = new OrdemServicoRepositoryFake(null);
        var orcamentoRepository = new OrcamentoRepositoryFake(quantidadeExistente: 0);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new CriarOrcamentoCommand(Guid.NewGuid(),
        [
            new CriarItemOrcamentoCommand(TipoItemOrcamento.Peca, "Correia", 1, 150m, Guid.NewGuid()),
        ]);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Null(orcamentoRepository.OrcamentoAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_SemItens_DeveLancarArgumentException()
    {
        var os = CriarOrdemServicoAguardandoOrcamento();
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(quantidadeExistente: 0);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new CriarOrcamentoCommand(os.Id, []);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Null(orcamentoRepository.OrcamentoAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComStatusOrdemServicoInvalido_DeveLancarRegraNegocio()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Solicitação", PrioridadeOrdemServico.Normal, null);
        os.IniciarDiagnostico(Guid.NewGuid());
        var ordemServicoRepository = new OrdemServicoRepositoryFake(os);
        var orcamentoRepository = new OrcamentoRepositoryFake(quantidadeExistente: 0);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrcamentoUseCase(ordemServicoRepository, orcamentoRepository, unitOfWork);
        var command = new CriarOrcamentoCommand(os.Id,
        [
            new CriarItemOrcamentoCommand(TipoItemOrcamento.Peca, "Correia", 1, 150m, Guid.NewGuid()),
        ]);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Null(orcamentoRepository.OrcamentoAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static OrdemServico CriarOrdemServicoAguardandoOrcamento()
    {
        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Solicitação", PrioridadeOrdemServico.Normal, null);
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnóstico concluído");
        return os;
    }

    private sealed class OrdemServicoRepositoryFake(OrdemServico? ordemServico) : IOrdemServicoRepository
    {
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(ordemServico);
    }

    private sealed class OrcamentoRepositoryFake(int quantidadeExistente) : IOrcamentoRepository
    {
        public Orcamento? OrcamentoAdicionado { get; private set; }

        public Task AdicionarAsync(Orcamento orcamento, CancellationToken ct)
        {
            OrcamentoAdicionado = orcamento;
            return Task.CompletedTask;
        }

        public Task<Orcamento?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<Orcamento?>(null);

        public Task<int> ContarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) =>
            Task.FromResult(quantidadeExistente);
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
