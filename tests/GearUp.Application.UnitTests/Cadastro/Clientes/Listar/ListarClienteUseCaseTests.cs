using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Clientes.Listar;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.UnitTests.Cadastro.Clientes.Listar;

public sealed class ListarClienteUseCaseTests
{
    [Fact]
    public async Task ListarAsync_ComClientes_DeveMapearTodos()
    {
        var cliente1 = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");
        var cliente2 = Cliente.Criar("Joao Pereira", "16899535009", "joao@email.com", "11888888888");
        var repository = new ClienteRepositoryFake([cliente1, cliente2]);
        var useCase = new ListarClienteUseCase(repository);

        var result = await useCase.ListarAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == cliente1.Id && c.Nome == "Maria da Silva");
        Assert.Contains(result, c => c.Id == cliente2.Id && c.Nome == "Joao Pereira");
    }

    [Fact]
    public async Task ListarAsync_SemClientes_DeveRetornarListaVazia()
    {
        var repository = new ClienteRepositoryFake([]);
        var useCase = new ListarClienteUseCase(repository);

        var result = await useCase.ListarAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    private sealed class ClienteRepositoryFake(IReadOnlyList<Cliente> clientes) : IClienteRepository
    {
        public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Cliente?>(null);
        public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken) => Task.FromResult(clientes);
        public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
