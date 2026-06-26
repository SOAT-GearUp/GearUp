using GearUp.Application.OrdemDeServico.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Ordens.Consultar;
using GearUp.Application.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.UnitTests.OrdemDeServico.Ordens.Consultar;

public sealed class ConsultarOrdemServicoUseCaseTests
{
    [Fact]
    public async Task ObterAsync_ComOrdemExistente_DeveRetornarDadosEOrcamentosMapeados()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordem = OrdemServico.Criar(clienteId, veiculoId, "Troca de óleo.", PrioridadeOrdemServico.Alta, null);

        var item = NovoItemOrcamento.Criar(TipoItemOrcamento.Servico, "Mão de obra", 1, 150m, null);
        var orcamento = Orcamento.Criar(ordem.Id, 1, [item]);

        var ordemRepository = new OrdemServicoRepositoryFake(ordem);
        var orcamentoRepository = new OrcamentoRepositoryFake([orcamento]);
        var useCase = new ConsultarOrdemServicoUseCase(ordemRepository, orcamentoRepository);

        var result = await useCase.ObterAsync(new ConsultarOrdemServicoCommand(ordem.Id), CancellationToken.None);

        Assert.Equal(ordem.Id, result.Id);
        Assert.Equal(clienteId, result.ClienteId);
        Assert.Equal(veiculoId, result.VeiculoId);
        Assert.Equal("Troca de óleo.", result.SolicitacaoInicial);
        Assert.Equal(StatusOrdemServico.Recebida, result.Status);
        Assert.Equal(PrioridadeOrdemServico.Alta, result.Prioridade);

        var orcamentoResult = Assert.Single(result.Orcamentos);
        Assert.Equal(orcamento.Id, orcamentoResult.Id);
        Assert.Equal(1, orcamentoResult.Versao);
        Assert.Equal(150m, orcamentoResult.ValorTotal);
        var itemResult = Assert.Single(orcamentoResult.Itens);
        Assert.Equal("Mão de obra", itemResult.Descricao);

        // A criação da OS registra o evento OS_CRIADA no histórico.
        Assert.Single(result.Historico);
    }

    [Fact]
    public async Task ObterAsync_SemOrcamentos_DeveRetornarColecaoVazia()
    {
        var ordem = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Revisão geral.", PrioridadeOrdemServico.Normal, null);

        var ordemRepository = new OrdemServicoRepositoryFake(ordem);
        var orcamentoRepository = new OrcamentoRepositoryFake([]);
        var useCase = new ConsultarOrdemServicoUseCase(ordemRepository, orcamentoRepository);

        var result = await useCase.ObterAsync(new ConsultarOrdemServicoCommand(ordem.Id), CancellationToken.None);

        Assert.Equal(ordem.Id, result.Id);
        Assert.Empty(result.Orcamentos);
    }

    [Fact]
    public async Task ObterAsync_ComOrdemInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var ordemRepository = new OrdemServicoRepositoryFake(ordem: null);
        var orcamentoRepository = new OrcamentoRepositoryFake([]);
        var useCase = new ConsultarOrdemServicoUseCase(ordemRepository, orcamentoRepository);

        var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.ObterAsync(new ConsultarOrdemServicoCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("OS_NAO_ENCONTRADA", excecao.Codigo);
    }

    private sealed class OrdemServicoRepositoryFake(OrdemServico? ordem) : IOrdemServicoRepository
    {
        public Task AdicionarAsync(OrdemServico ordem, CancellationToken ct) => Task.CompletedTask;
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult(ordem);
        public Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct) => Task.FromResult<IReadOnlyList<OrdemServico>>([]);
    }

    private sealed class OrcamentoRepositoryFake(IReadOnlyList<Orcamento> orcamentos) : IOrcamentoRepository
    {
        public Task<IReadOnlyList<Orcamento>> ListarPorOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct) => Task.FromResult(orcamentos);
    }
}
