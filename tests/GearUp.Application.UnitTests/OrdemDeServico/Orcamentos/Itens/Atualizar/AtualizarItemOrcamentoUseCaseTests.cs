using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Itens.Atualizar;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.UnitTests.OrdemDeServico.Orcamentos.Itens.Atualizar;

public sealed class AtualizarItemOrcamentoUseCaseTests
{
    [Fact]
    public async Task AtualizarAsync_ComItemExistenteEOrcamentoPendente_DeveAtualizarESalvar()
    {
        var orcamento = CriarOrcamentoPendente();
        var item = orcamento.Itens.First();
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AtualizarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, item.Id, TipoItemOrcamento.Peca, "Correia nova", 3, 200m, Guid.NewGuid());

        await useCase.AtualizarAsync(command, CancellationToken.None);

        var atualizado = orcamento.Itens.Single(x => x.Id == item.Id);
        Assert.Equal("Correia nova", atualizado.Descricao);
        Assert.Equal(3, atualizado.Quantidade);
        Assert.Equal(200m, atualizado.ValorUnitario);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarAsync_ComOrcamentoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var orcamentoRepository = new OrcamentoRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AtualizarItemOrcamentoCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoItemOrcamento.Peca, "Correia", 1, 100m, Guid.NewGuid());

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AtualizarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarAsync_ComItemInexistente_DeveLancarRegraNegocio()
    {
        var orcamento = CriarOrcamentoPendente();
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AtualizarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, Guid.NewGuid(), TipoItemOrcamento.Peca, "Correia", 1, 100m, Guid.NewGuid());

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.AtualizarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarAsync_ComOrcamentoJaDecidido_DeveLancarRegraNegocio()
    {
        var orcamento = CriarOrcamentoPendente();
        var item = orcamento.Itens.First();
        orcamento.Decidir(aprovado: true);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AtualizarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, item.Id, TipoItemOrcamento.Peca, "Correia", 1, 100m, Guid.NewGuid());

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.AtualizarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarAsync_ComDadosInvalidos_DeveLancarArgumentException()
    {
        var orcamento = CriarOrcamentoPendente();
        var item = orcamento.Itens.First();
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        // Quantidade zero é inválida em NovoItemOrcamento.Criar.
        var command = new AtualizarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, item.Id, TipoItemOrcamento.Peca, "Correia", 0, 100m, Guid.NewGuid());

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.AtualizarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static Orcamento CriarOrcamentoPendente() =>
        Orcamento.Criar(Guid.NewGuid(), 1,
        [
            NovoItemOrcamento.Criar(TipoItemOrcamento.Peca, "Correia", 1, 150m, Guid.NewGuid()),
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
