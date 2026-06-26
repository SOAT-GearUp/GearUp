using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Clientes.Excluir;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.UnitTests.Cadastro.Clientes.Excluir;

public sealed class ExcluirClienteUseCaseTests
{
    [Fact]
    public async Task ExcluirAsync_ComClienteExistente_DeveExcluirESalvar()
    {
        var cliente = CriarCliente();
        var repository = new ClienteRepositoryFake(cliente);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new ExcluirClienteUseCase(repository, unitOfWork);

        await useCase.ExcluirAsync(cliente.Id, CancellationToken.None);

        Assert.False(cliente.Ativo);
        Assert.NotNull(cliente.ExcluidoEm);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task ExcluirAsync_ComClienteInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new ClienteRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new ExcluirClienteUseCase(repository, unitOfWork);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.ExcluirAsync(Guid.NewGuid(), CancellationToken.None));

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
