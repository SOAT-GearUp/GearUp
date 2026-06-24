using GearUp.Application.Atendimento.Comum.Interfaces;
using GearUp.Application.Clientes.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.Atendimento.Criar;

internal sealed class CriarOrdemServicoUseCase(
    IClienteRepository clienteRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork) : ICriarOrdemServicoUseCase
{
    public async Task<CriarOrdemServicoResult> CriarAsync(CriarOrdemServicoCommand command, CancellationToken ct)
    {
        var cliente = await clienteRepository.ObterAsync(command.ClienteId, ct)
            ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

        if (cliente.Veiculos.All(v => v.Id != command.VeiculoId))
            throw new RecursoNaoEncontradoException("VEICULO_NAO_ENCONTRADO", "Veículo não pertence ao cliente.");

        var ordem = OrdemServico.Criar(command.ClienteId, command.VeiculoId, command.SolicitacaoInicial, command.Prioridade, command.Prazo);

        await ordemServicoRepository.AdicionarAsync(ordem, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return new CriarOrdemServicoResult(ordem.Id);
    }
}
