using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Itens.Adicionar;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.UnitTests.OrdemDeServico.Orcamentos.Itens.Adicionar;

public sealed class AdicionarItemOrcamentoUseCaseTests
{
    [Fact]
    public async Task AdicionarAsync_ComOrcamentoPendente_DeveAdicionarItemESalvar()
    {
        var orcamento = CriarOrcamentoPendente();
        var quantidadeInicial = orcamento.Itens.Count;
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AdicionarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AdicionarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, TipoItemOrcamento.MaoDeObra, "Troca de óleo", 1, 80m, null);

        await useCase.AdicionarAsync(command, CancellationToken.None);

        Assert.Equal(quantidadeInicial + 1, orcamento.Itens.Count);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AdicionarAsync_ComOrcamentoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var orcamentoRepository = new OrcamentoRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AdicionarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AdicionarItemOrcamentoCommand(
            Guid.NewGuid(), Guid.NewGuid(), TipoItemOrcamento.MaoDeObra, "Troca de óleo", 1, 80m, null);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AdicionarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AdicionarAsync_ComOrcamentoJaDecidido_DeveLancarRegraNegocio()
    {
        var orcamento = CriarOrcamentoPendente();
        orcamento.Decidir(aprovado: true);
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AdicionarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        var command = new AdicionarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, TipoItemOrcamento.MaoDeObra, "Troca de óleo", 1, 80m, null);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.AdicionarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AdicionarAsync_ComItemInvalido_DeveLancarRegraNegocio()
    {
        var orcamento = CriarOrcamentoPendente();
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AdicionarItemOrcamentoUseCase(orcamentoRepository, unitOfWork);
        // Serviço/Mão de obra não pode estar vinculado a item de estoque.
        var command = new AdicionarItemOrcamentoCommand(
            orcamento.OrdemServicoId, orcamento.Id, TipoItemOrcamento.Servico, "Alinhamento", 1, 80m, Guid.NewGuid());

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.AdicionarAsync(command, CancellationToken.None));

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
