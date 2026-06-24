using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Criar;

internal sealed class CriarOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : ICriarOrcamentoUseCase
{
    public async Task<CriarOrcamentoResult> CriarAsync(CriarOrcamentoCommand command, CancellationToken ct)
    {
        var ordemServico = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        var itens = command.Itens
            .Select(item => NovoItemOrcamento.Criar(item.Tipo, item.Descricao, item.Quantidade, item.ValorUnitario, item.EstoqueItemId))
            .ToList();

        var orcamento = ordemServico.CriarOrcamento(itens);

        await unitOfWork.SaveChangesAsync(ct);

        return new CriarOrcamentoResult(orcamento.Id, orcamento.Versao, orcamento.ValorTotal);
    }
}
