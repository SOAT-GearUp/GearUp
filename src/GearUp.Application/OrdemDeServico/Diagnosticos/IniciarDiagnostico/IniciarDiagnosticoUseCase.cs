using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;

namespace GearUp.Application.OrdemDeServico.Diagnosticos.IniciarDiagnostico;

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
