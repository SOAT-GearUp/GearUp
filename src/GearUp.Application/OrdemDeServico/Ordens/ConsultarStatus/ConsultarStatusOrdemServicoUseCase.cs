using GearUp.Application.OrdemDeServico.Common.Interfaces;

namespace GearUp.Application.OrdemDeServico.Ordens.ConsultarStatus;

internal sealed class ConsultarStatusOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IConsultarStatusOrdemServicoUseCase
{
    public async Task<ConsultarStatusOrdemServicoResult> ObterAsync(ConsultarStatusOrdemServicoCommand command, CancellationToken ct)
    {
        var ordemServico = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        return new ConsultarStatusOrdemServicoResult(
            ordemServico.Id,
            ordemServico.ClienteId,
            ordemServico.Status);
    }
}
