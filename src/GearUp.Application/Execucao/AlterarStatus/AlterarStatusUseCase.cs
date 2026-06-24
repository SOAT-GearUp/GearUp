using GearUp.Application.Common.Interfaces;
using GearUp.Application.Execucao.Comum.Interfaces;

namespace GearUp.Application.Execucao.AlterarStatus;

internal sealed class AlterarStatusUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork) : IAlterarStatusUseCase
{
    public async Task AlterarAsync(AlterarStatusCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.Id, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        os.AlterarStatus(command.Status);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
