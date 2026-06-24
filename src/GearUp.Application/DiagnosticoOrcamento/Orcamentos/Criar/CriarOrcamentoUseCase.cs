using GearUp.Application.Common.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Common.Interfaces;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Criar;

internal sealed class CriarOrcamentoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IOrcamentoRepository orcamentoRepository,
    IUnitOfWork unitOfWork) : ICriarOrcamentoUseCase
{
    public async Task<CriarOrcamentoResult> CriarAsync(CriarOrcamentoCommand command, CancellationToken ct)
    {
        var ordemServico = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        var versao = await orcamentoRepository.ContarPorOrdemServicoAsync(command.OrdemServicoId, ct) + 1;

        var itens = command.Itens
            .Select(item => NovoItemOrcamento.Criar(item.Tipo, item.Descricao, item.Quantidade, item.ValorUnitario, item.EstoqueItemId))
            .ToList();

        var orcamento = Orcamento.Criar(command.OrdemServicoId, versao, itens);

        ordemServico.AguardarAprovacao(orcamento.Id, orcamento.Versao);

        await orcamentoRepository.AdicionarAsync(orcamento, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new CriarOrcamentoResult(orcamento.Id, orcamento.Versao, orcamento.ValorTotal);
    }
}
