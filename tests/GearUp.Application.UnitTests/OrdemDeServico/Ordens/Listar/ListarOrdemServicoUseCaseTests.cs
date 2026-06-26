using GearUp.Application.OrdemDeServico.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Ordens.Listar;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.OrdemDeServico.Ordens.Listar;

public sealed class ListarOrdemServicoUseCaseTests
{
    [Fact]
    public async Task ListarAsync_ComOrdens_DeveMapearTodasAsOrdens()
    {
        var ordem1 = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Solicitação 1.", PrioridadeOrdemServico.Normal, null);
        var prazo = DateTimeOffset.UtcNow.AddDays(3);
        var ordem2 = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Solicitação 2.", PrioridadeOrdemServico.Urgente, prazo);

        var repository = new OrdemServicoRepositoryFake([ordem1, ordem2]);
        var useCase = new ListarOrdemServicoUseCase(repository);

        var result = await useCase.ListarAsync(new ListarOrdemServicoCommand(EmAndamento: false, ClienteId: null), CancellationToken.None);

        Assert.Equal(2, result.Count);

        var primeira = result[0];
        Assert.Equal(ordem1.Id, primeira.Id);
        Assert.Equal(ordem1.ClienteId, primeira.ClienteId);
        Assert.Equal(ordem1.VeiculoId, primeira.VeiculoId);
        Assert.Equal(StatusOrdemServico.Recebida, primeira.Status);
        Assert.Equal(PrioridadeOrdemServico.Normal, primeira.Prioridade);
        Assert.Null(primeira.Prazo);

        var segunda = result[1];
        Assert.Equal(ordem2.Id, segunda.Id);
        Assert.Equal(PrioridadeOrdemServico.Urgente, segunda.Prioridade);
        Assert.Equal(prazo, segunda.Prazo);
    }

    [Fact]
    public async Task ListarAsync_SemOrdens_DeveRetornarColecaoVazia()
    {
        var repository = new OrdemServicoRepositoryFake([]);
        var useCase = new ListarOrdemServicoUseCase(repository);

        var result = await useCase.ListarAsync(new ListarOrdemServicoCommand(EmAndamento: true, ClienteId: null), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListarAsync_DeveRepassarFiltrosAoRepositorio()
    {
        var clienteId = Guid.NewGuid();
        var repository = new OrdemServicoRepositoryFake([]);
        var useCase = new ListarOrdemServicoUseCase(repository);

        await useCase.ListarAsync(new ListarOrdemServicoCommand(EmAndamento: true, ClienteId: clienteId), CancellationToken.None);

        Assert.True(repository.SomenteEmAndamentoRecebido);
        Assert.Equal(clienteId, repository.ClienteIdRecebido);
    }

    private sealed class OrdemServicoRepositoryFake(IReadOnlyList<OrdemServico> ordens) : IOrdemServicoRepository
    {
        public bool? SomenteEmAndamentoRecebido { get; private set; }
        public Guid? ClienteIdRecebido { get; private set; }

        public Task AdicionarAsync(OrdemServico ordem, CancellationToken ct) => Task.CompletedTask;
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult<OrdemServico?>(null);

        public Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct)
        {
            SomenteEmAndamentoRecebido = somenteEmAndamento;
            ClienteIdRecebido = clienteId;
            return Task.FromResult(ordens);
        }
    }
}
