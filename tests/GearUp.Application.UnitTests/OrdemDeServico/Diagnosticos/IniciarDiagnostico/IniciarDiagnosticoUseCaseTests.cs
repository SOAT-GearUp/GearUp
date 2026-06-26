using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.IniciarDiagnostico;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.OrdemDeServico.Diagnosticos.IniciarDiagnostico;

public sealed class IniciarDiagnosticoUseCaseTests
{
    [Fact]
    public async Task IniciarAsync_ComOrdemServicoRecebida_DeveIniciarDiagnosticoESalvar()
    {
        var os = CriarOrdemServicoRecebida();
        var repository = new OrdemServicoRepositoryFake(os);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new IniciarDiagnosticoUseCase(repository, unitOfWork);
        var mecanicoId = Guid.NewGuid();
        var command = new IniciarDiagnosticoCommand(os.Id, mecanicoId);

        await useCase.IniciarAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
        Assert.Equal(mecanicoId, os.MecanicoId);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task IniciarAsync_ComOrdemServicoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new OrdemServicoRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new IniciarDiagnosticoUseCase(repository, unitOfWork);
        var command = new IniciarDiagnosticoCommand(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.IniciarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task IniciarAsync_ComStatusDiferenteDeRecebida_DeveLancarRegraNegocio()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico(Guid.NewGuid());
        var repository = new OrdemServicoRepositoryFake(os);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new IniciarDiagnosticoUseCase(repository, unitOfWork);
        var command = new IniciarDiagnosticoCommand(os.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.IniciarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static OrdemServico CriarOrdemServicoRecebida() =>
        OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Barulho no motor", PrioridadeOrdemServico.Normal, null);

    private sealed class OrdemServicoRepositoryFake(OrdemServico? ordemServico) : IOrdemServicoRepository
    {
        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(ordemServico);
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
