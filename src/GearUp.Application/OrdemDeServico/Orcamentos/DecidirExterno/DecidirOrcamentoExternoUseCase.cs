using GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Orcamentos.Decidir;

namespace GearUp.Application.OrdemDeServico.Orcamentos.DecidirExterno;

internal sealed class DecidirOrcamentoExternoUseCase(
    IOrcamentoRepository orcamentoRepository,
    IDecidirOrcamentoUseCase decidirOrcamentoUseCase) : IDecidirOrcamentoExternoUseCase
{
    public async Task DecidirAsync(DecidirOrcamentoExternoCommand command, CancellationToken ct)
    {
        var orcamento = await orcamentoRepository.ObterAsync(command.OrcamentoId, ct)
            ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        await decidirOrcamentoUseCase.DecidirAsync(
            new DecidirOrcamentoCommand(orcamento.OrdemServicoId, orcamento.Id, command.Aprovado),
            ct);
    }
}
