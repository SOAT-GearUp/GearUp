using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;

namespace GearUp.Application.DiagnosticoOrcamento.IniciarDiagnostico;

internal sealed class IniciarDiagnosticoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IIniciarDiagnosticoUseCase
{
    public async Task IniciarAsync(IniciarDiagnosticoCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        os.IniciarDiagnostico(command.MecanicoId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
