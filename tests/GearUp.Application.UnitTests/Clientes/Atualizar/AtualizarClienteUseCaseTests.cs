using GearUp.Application.Atendimento.Clientes.Atualizar;
using GearUp.Application.Atendimento.Clientes.Common.Interfaces;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.UnitTests.Clientes.Atualizar;

public sealed class AtualizarClienteUseCaseTests
{
    [Fact]
    public async Task AtualizarAsync_ComClienteExistente_DeveAtualizarESalvar()
    {
        var cliente = CriarCliente();
        var repository = new ClienteRepositoryFake(cliente);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarClienteUseCase(repository, unitOfWork);
        var command = new AtualizarClienteCommand(cliente.Id, "Maria Souza", "maria.souza@email.com", "11888888888");

        await useCase.AtualizarAsync(command, CancellationToken.None);

        Assert.Equal("Maria Souza", cliente.Nome);
        Assert.Equal("maria.souza@email.com", cliente.Email.Endereco);
        Assert.Equal("11888888888", cliente.Telefone.Numero);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task AtualizarAsync_ComClienteInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new ClienteRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new AtualizarClienteUseCase(repository, unitOfWork);
        var command = new AtualizarClienteCommand(Guid.NewGuid(), "Maria Souza", "maria.souza@email.com", "11888888888");

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.AtualizarAsync(command, CancellationToken.None));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", ex.Codigo);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static Cliente CriarCliente() =>
        Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");

    private sealed class ClienteRepositoryFake(Cliente? cliente) : IClienteRepository
    {
        public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(cliente);
        public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Cliente>>([]);
        public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken) => Task.CompletedTask;
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
