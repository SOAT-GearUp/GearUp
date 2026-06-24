using GearUp.Application.Common.Interfaces;
using GearUp.Application.Execucao.Comum.Interfaces;
using GearUp.Domain.DomainEvents.Execucao;

namespace GearUp.Application.Execucao.AlterarStatus;

internal sealed class AlterarStatusUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IOrcamentoRepository orcamentoRepository,
    IUnitOfWork unitOfWork) : IAlterarStatusUseCase
{
    public async Task AlterarAsync(AlterarStatusCommand command, CancellationToken ct)
    {
        var os = await ordemServicoRepository.ObterAsync(command.Id, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        if (command.Status == Domain.Enums.StatusOrdemServico.EmExecucao)
        {
            var orcamento = await orcamentoRepository.ObterAprovadoPorOrdemServicoAsync(os.Id, ct)
                ?? throw new RecursoNaoEncontradoException("ORCAMENTO_APROVADO_NAO_ENCONTRADO", "Nenhum orçamento aprovado encontrado para a OS.");

            var itens = orcamento.Itens
                .Where(i => i.EstoqueItemId.HasValue)
                .Select(i => new ItemDeEstoque(i.EstoqueItemId!.Value, i.Quantidade, i.Descricao))
                .ToList();

            os.IniciarExecucao(itens);
        }
        else
        {
            os.AlterarStatus(command.Status);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
