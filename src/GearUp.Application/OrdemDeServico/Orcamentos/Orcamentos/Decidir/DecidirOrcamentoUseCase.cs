using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;

namespace GearUp.Application.OrdemDeServico.Orcamentos.Decidir;

internal sealed class DecidirOrcamentoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IOrcamentoRepository orcamentoRepository,
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
        os.ReceberDecisaoOrcamento(orcamento.Id, command.Aprovado);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
