using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Atualizar;

internal sealed class AtualizarItemOrcamentoUseCase(IOrcamentoRepository orcamentoRepository, IUnitOfWork unitOfWork) : IAtualizarItemOrcamentoUseCase
{
    public async Task AtualizarAsync(AtualizarItemOrcamentoCommand command, CancellationToken ct)
    {
        var orcamento = await orcamentoRepository.ObterAsync(command.OrcamentoId, ct)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        if (orcamento.OrdemServicoId != command.OrdemServicoId)
            throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado para a ordem de serviço informada.");

        orcamento.AtualizarItem(command.ItemId, NovoItemOrcamento.Criar(command.Tipo, command.Descricao, command.Quantidade, command.ValorUnitario, command.EstoqueItemId));

        await unitOfWork.SaveChangesAsync(ct);
    }
}
