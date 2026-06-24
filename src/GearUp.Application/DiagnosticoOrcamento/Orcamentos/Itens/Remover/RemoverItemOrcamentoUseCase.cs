using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Common.Interfaces;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Remover;

internal sealed class RemoverItemOrcamentoUseCase(IOrcamentoRepository orcamentoRepository, IUnitOfWork unitOfWork) : IRemoverItemOrcamentoUseCase
{
    public async Task RemoverAsync(RemoverItemOrcamentoCommand command, CancellationToken ct)
    {
        var orcamento = await orcamentoRepository.ObterAsync(command.OrcamentoId, ct)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        orcamento.RemoverItem(command.ItemId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
