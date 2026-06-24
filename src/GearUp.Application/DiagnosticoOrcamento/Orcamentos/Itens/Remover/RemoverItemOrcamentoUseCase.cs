using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Remover;

internal sealed class RemoverItemOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IRemoverItemOrcamentoUseCase
{
    public async Task RemoverAsync(RemoverItemOrcamentoCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        var orcamento = os.Orcamentos.SingleOrDefault(x => x.Id == command.OrcamentoId)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        orcamento.RemoverItem(command.ItemId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
