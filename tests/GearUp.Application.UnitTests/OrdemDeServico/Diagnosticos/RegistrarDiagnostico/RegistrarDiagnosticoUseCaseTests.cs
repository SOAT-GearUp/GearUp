using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.RegistrarDiagnostico;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.OrdemDeServico.Diagnosticos.RegistrarDiagnostico;

public sealed class RegistrarDiagnosticoUseCaseTests
{
    [Fact]
    public async Task RegistrarAsync_ComOrdemServicoEmDiagnostico_DeveRegistrarESalvar()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var repository = new OrdemServicoRepositoryFake(os);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RegistrarDiagnosticoUseCase(repository, unitOfWork);
        var command = new RegistrarDiagnosticoCommand(os.Id, "Correia dentada desgastada");

        await useCase.RegistrarAsync(command, CancellationToken.None);

        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, os.Status);
        Assert.Equal("Correia dentada desgastada", os.Diagnostico);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task RegistrarAsync_ComOrdemServicoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var repository = new OrdemServicoRepositoryFake(null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RegistrarDiagnosticoUseCase(repository, unitOfWork);
        var command = new RegistrarDiagnosticoCommand(Guid.NewGuid(), "Diagnóstico");

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.RegistrarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task RegistrarAsync_ComStatusDiferenteDeEmDiagnostico_DeveLancarRegraNegocio()
    {
        var os = CriarOrdemServicoRecebida();
        var repository = new OrdemServicoRepositoryFake(os);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RegistrarDiagnosticoUseCase(repository, unitOfWork);
        var command = new RegistrarDiagnosticoCommand(os.Id, "Diagnóstico");

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => useCase.RegistrarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task RegistrarAsync_ComDiagnosticoVazio_DeveLancarArgumentException()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var repository = new OrdemServicoRepositoryFake(os);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new RegistrarDiagnosticoUseCase(repository, unitOfWork);
        var command = new RegistrarDiagnosticoCommand(os.Id, "   ");

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.RegistrarAsync(command, CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static OrdemServico CriarOrdemServicoRecebida() =>
        OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Barulho no motor", PrioridadeOrdemServico.Normal, null);

    private static OrdemServico CriarOrdemServicoEmDiagnostico()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico(Guid.NewGuid());
        return os;
    }

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
