using GearUp.Application.Clientes;
using GearUp.Application.Clientes.Cadastrar;
using GearUp.Application.Common;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.UnitTests.Clientes;

public sealed class CadastrarClienteUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_ComDocumentoNovo_DeveAdicionarESalvar()
    {
        var repository = new ClienteRepositoryFake(documentoExiste: false);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarClienteUseCase(repository, unitOfWork);
        var command = CriarCommand();

        var result = await useCase.ExecutarAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(repository.ClienteAdicionado);
        Assert.Equal(result.Id, repository.ClienteAdicionado.Id);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task ExecutarAsync_ComDocumentoDuplicado_DeveRejeitarCadastro()
    {
        var repository = new ClienteRepositoryFake(documentoExiste: true);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CadastrarClienteUseCase(repository, unitOfWork);
        var command = CriarCommand();

        await Assert.ThrowsAsync<ClienteDocumentoDuplicadoException>(
            () => useCase.ExecutarAsync(command, CancellationToken.None));

        Assert.Null(repository.ClienteAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static CadastrarClienteCommand CriarCommand() =>
        new(
            "Maria da Silva",
            "52998224725",
            "maria@email.com",
            "11999999999");

    private sealed class ClienteRepositoryFake(bool documentoExiste)
        : IClienteRepository
    {
        public Cliente? ClienteAdicionado { get; private set; }

        public Task<bool> DocumentoExisteAsync(
            Documento documento,
            CancellationToken cancellationToken) =>
            Task.FromResult(documentoExiste);

        public Task AdicionarAsync(
            Cliente cliente,
            CancellationToken cancellationToken)
        {
            ClienteAdicionado = cliente;
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveChangesChamadas { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesChamadas++;
            return Task.FromResult(1);
        }
    }
}
