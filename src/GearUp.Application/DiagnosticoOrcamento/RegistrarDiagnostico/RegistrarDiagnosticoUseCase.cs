using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;

namespace GearUp.Application.DiagnosticoOrcamento.RegistrarDiagnostico;

internal sealed class RegistrarDiagnosticoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IRegistrarDiagnosticoUseCase
{
    public async Task RegistrarAsync(RegistrarDiagnosticoCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        os.RegistrarDiagnostico(command.Diagnostico);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
