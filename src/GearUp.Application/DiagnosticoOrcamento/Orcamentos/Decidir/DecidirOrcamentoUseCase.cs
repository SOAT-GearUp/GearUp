using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Decidir;

internal sealed class DecidirOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IDecidirOrcamentoUseCase
{
    public async Task DecidirAsync(DecidirOrcamentoCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        os.DecidirOrcamento(command.OrcamentoId, command.Aprovado);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
