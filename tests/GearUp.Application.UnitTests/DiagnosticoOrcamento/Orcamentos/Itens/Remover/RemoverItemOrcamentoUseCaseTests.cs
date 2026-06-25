using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Remover;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.UnitTests.DiagnosticoOrcamento.Orcamentos.Itens.Remover;

public sealed class RemoverItemOrcamentoUseCaseTests
{
    [Fact]
    public async Task RemoverAsync_ComItemExistenteEOrcamentoPendente_DeveRemoverESalvar()
    {
        var orcamento = CriarOrcamentoComDoisItens();
        var item = orcamento.Itens.First();
        var quantidadeInicial = orcamento.Itens.Count;
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RemoverItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new RemoverItemOrcamentoCommand(Guid.NewGuid(), orcamento.Id, item.Id);

        await useCase.RemoverAsync(command, CancellationToken.None);

        Assert.Equal(quantidadeInicial - 1, orcamento.Itens.Count);
        Assert.DoesNotContain(orcamento.Itens, x => x.Id == item.Id);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task RemoverAsync_ComOrcamentoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var orcamentoRepository = new OrcamentoRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RemoverItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new RemoverItemOrcamentoCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.RemoverAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task RemoverAsync_ComItemInexistente_DeveLancarRegraNegocio()
    {
        var orcamento = CriarOrcamentoComDoisItens();
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RemoverItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new RemoverItemOrcamentoCommand(Guid.NewGuid(), orcamento.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.RemoverAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task RemoverAsync_ComOrcamentoJaDecidido_DeveLancarRegraNegocio()
    {
        var orcamento = CriarOrcamentoComDoisItens();
        var item = orcamento.Itens.First();
        orcamento.Decidir(aprovado: true);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RemoverItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new RemoverItemOrcamentoCommand(Guid.NewGuid(), orcamento.Id, item.Id);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.RemoverAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static Orcamento CriarOrcamentoComDoisItens() =>
        Orcamento.Criar(Guid.NewGuid(), 1,
        [
            NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, "Correia", 1, 150m, Guid.NewGuid()),
            NovoItemOrcamento.Criar(TipoItemOrcamento.MaoDeObra, "Troca", 2, 100m, null),
        ]);

    private sealed class OrcamentoRepositoryFake(Orcamento? orcamento) : IOrcamentoRepository
    {
        public Task AdicionarAsync(Orcamento orcamento, CancellationToken ct) => Task.CompletedTask;

        public Task<Orcamento?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(orcamento);

        public Task<int> ContarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) =>
            Task.FromResult(0);
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
