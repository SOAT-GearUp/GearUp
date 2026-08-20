using GearUp.Application.Common.Exceptions;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Decidir;
using GearUp.Application.OrdemDeServico.Orcamentos.DecidirExterno;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.UnitTests.OrdemDeServico.Orcamentos.DecidirExterno;

public sealed class DecidirOrcamentoExternoUseCaseTests
{
    [Fact]
    public async Task DecidirAsync_ComOrcamentoExistente_DeveDelegarComOrdemServicoDoOrcamento()
    {
        var orcamento = CriarOrcamento();
        var repository = new OrcamentoRepositoryFake(orcamento);
        var decidirUseCase = new DecidirOrcamentoUseCaseFake();
        var useCase = new DecidirOrcamentoExternoUseCase(repository, decidirUseCase);

        await useCase.DecidirAsync(new DecidirOrcamentoExternoCommand(orcamento.Id, Aprovado: true), CancellationToken.None);

        Assert.NotNull(decidirUseCase.CommandRecebido);
        Assert.Equal(orcamento.OrdemServicoId, decidirUseCase.CommandRecebido.OrdemServicoId);
        Assert.Equal(orcamento.Id, decidirUseCase.CommandRecebido.OrcamentoId);
        Assert.True(decidirUseCase.CommandRecebido.Aprovado);
    }

    [Fact]
    public async Task DecidirAsync_ComOrcamentoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new OrcamentoRepositoryFake(orcamento: null);
        var decidirUseCase = new DecidirOrcamentoUseCaseFake();
        var useCase = new DecidirOrcamentoExternoUseCase(repository, decidirUseCase);

        var exception = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.DecidirAsync(new DecidirOrcamentoExternoCommand(Guid.NewGuid(), Aprovado: false), CancellationToken.None));

        Assert.Equal("ORCAMENTO_NAO_ENCONTRADO", exception.Codigo);
        Assert.Null(decidirUseCase.CommandRecebido);
    }

    private static Orcamento CriarOrcamento()
    {
        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Troca de oleo", 1, 120m, null);
        return Orcamento.Criar(Guid.NewGuid(), 1, [item]);
    }

    private sealed class OrcamentoRepositoryFake(Orcamento? orcamento) : IOrcamentoRepository
    {
        public Task AdicionarAsync(Orcamento novoOrcamento, CancellationToken ct) => Task.CompletedTask;
        public Task<Orcamento?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult(orcamento);
        public Task<int> ContarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) => Task.FromResult(0);
    }

    private sealed class DecidirOrcamentoUseCaseFake : IDecidirOrcamentoUseCase
    {
        public DecidirOrcamentoCommand? CommandRecebido { get; private set; }

        public Task DecidirAsync(DecidirOrcamentoCommand command, CancellationToken ct)
        {
            CommandRecebido = command;
            return Task.CompletedTask;
        }
    }
}
