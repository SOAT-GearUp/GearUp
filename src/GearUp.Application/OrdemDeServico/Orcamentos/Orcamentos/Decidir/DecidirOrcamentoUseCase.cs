using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.OrdemDeServico.Orcamentos.Decidir;

internal sealed class DecidirOrcamentoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IOrcamentoRepository orcamentoRepository,
    IEstoqueRepository estoqueRepository,
    IUnitOfWork unitOfWork) : IDecidirOrcamentoUseCase
{
    public async Task DecidirAsync(DecidirOrcamentoCommand command, CancellationToken ct)
    {
        var orcamento = await orcamentoRepository.ObterAsync(command.OrcamentoId, ct)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        if (orcamento.OrdemServicoId != command.OrdemServicoId)
            throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado para a ordem de serviço informada.");

        var os = await ordemServicoRepository.ObterAsync(orcamento.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        orcamento.Decidir(command.Aprovado);

        var estoqueDisponivel = !command.Aprovado || await EstoqueDisponivelParaExecucaoAsync(orcamento, ct);
        os.ReceberDecisaoOrcamento(orcamento.Id, command.Aprovado, estoqueDisponivel);

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<bool> EstoqueDisponivelParaExecucaoAsync(Orcamento orcamento, CancellationToken ct)
    {
        var itensEstoque = orcamento.Itens.Where(i => i.EstoqueItemId.HasValue);

        foreach (var item in itensEstoque)
        {
            var estoque = await estoqueRepository.ObterAsync(item.EstoqueItemId!.Value, ct);
            if (estoque is null || estoque.QuantidadeDisponivel < item.Quantidade)
                return false;
        }

        return true;
    }
}
