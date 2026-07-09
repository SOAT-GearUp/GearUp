using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Adicionar;

internal sealed class AdicionarItemOrcamentoUseCase(IOrcamentoRepository orcamentoRepository, IUnitOfWork unitOfWork) : IAdicionarItemOrcamentoUseCase
{
    public async Task AdicionarAsync(AdicionarItemOrcamentoCommand command, CancellationToken ct)
    {
        var orcamento = await orcamentoRepository.ObterAsync(command.OrcamentoId, ct)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        if (orcamento.OrdemServicoId != command.OrdemServicoId)
            throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado para a ordem de serviço informada.");

        orcamento.AdicionarItem(NovoItemOrcamento.Criar(command.Tipo, command.Descricao, command.Quantidade, command.ValorUnitario, command.EstoqueItemId));

        await unitOfWork.SaveChangesAsync(ct);
    }
}
