using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Atualizar;

internal sealed class AtualizarItemOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IAtualizarItemOrcamentoUseCase
{
    public async Task AtualizarAsync(AtualizarItemOrcamentoCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        var orcamento = os.Orcamentos.SingleOrDefault(x => x.Id == command.OrcamentoId)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        orcamento.AtualizarItem(command.ItemId, NovoItemOrcamento.Criar(command.Tipo, command.Descricao, command.Quantidade, command.ValorUnitario, command.EstoqueItemId));

        await unitOfWork.SaveChangesAsync(ct);
    }
}
