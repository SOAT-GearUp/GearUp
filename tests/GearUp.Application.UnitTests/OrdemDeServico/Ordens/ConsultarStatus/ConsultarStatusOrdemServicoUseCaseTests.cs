using GearUp.Application.OrdemDeServico.Common.Interfaces;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.OrdemDeServico.Ordens.ConsultarStatus;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.OrdemDeServico.Ordens.ConsultarStatus;

public sealed class ConsultarStatusOrdemServicoUseCaseTests
{
    [Fact]
    public async Task ObterAsync_ComOrdemExistente_DeveRetornarStatus()
    {
        var ordem = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Troca de oleo", PrioridadeOrdemServico.Normal, null);
        var repository = new OrdemServicoRepositoryFake(ordem);
        var useCase = new ConsultarStatusOrdemServicoUseCase(repository);

        var result = await useCase.ObterAsync(new ConsultarStatusOrdemServicoCommand(ordem.Id), CancellationToken.None);

        Assert.Equal(ordem.Id, result.OrdemServicoId);
        Assert.Equal(ordem.ClienteId, result.ClienteId);
        Assert.Equal(StatusOrdemServico.Recebida, result.Status);
    }

    [Fact]
    public async Task ObterAsync_ComOrdemInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new OrdemServicoRepositoryFake(ordem: null);
        var useCase = new ConsultarStatusOrdemServicoUseCase(repository);

        var exception = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.ObterAsync(new ConsultarStatusOrdemServicoCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("OS_NAO_ENCONTRADA", exception.Codigo);
    }

    private sealed class OrdemServicoRepositoryFake(OrdemServico? ordem) : IOrdemServicoRepository
    {
        public Task AdicionarAsync(OrdemServico ordemServico, CancellationToken ct) => Task.CompletedTask;
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult(ordem);
        public Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct) => Task.FromResult<IReadOnlyList<OrdemServico>>([]);
    }
}
