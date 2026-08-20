using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.OrdemDeServico.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Application.Cadastro.Veiculos.Common.Interfaces;
using GearUp.Domain.ValueObjects.Orcamentos;

using IOrcamentoRepository = GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces.IOrcamentoRepository;

namespace GearUp.Application.OrdemDeServico.Ordens.Criar;

internal sealed class CriarOrdemServicoUseCase(
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IOrcamentoRepository orcamentoRepository,
    IUnitOfWork unitOfWork) : ICriarOrdemServicoUseCase
{
    public async Task<CriarOrdemServicoResult> CriarAsync(CriarOrdemServicoCommand command, CancellationToken ct)
    {
        var cliente = await clienteRepository.ObterAsync(command.ClienteId, ct)
            ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

        var veiculo = await veiculoRepository.ObterAsync(command.VeiculoId, ct);

        if (veiculo is null || veiculo.ClienteId != cliente.Id)
            throw new RecursoNaoEncontradoException("VEICULO_NAO_ENCONTRADO", "Veículo não pertence ao cliente.");

        var ordem = OrdemServico.Criar(command.ClienteId, command.VeiculoId, command.SolicitacaoInicial, command.Prioridade, command.Prazo);

        await ordemServicoRepository.AdicionarAsync(ordem, ct);

        if (command.Itens is { Count: > 0 })
        {
            var itens = command.Itens
                .Select(item => NovoItemOrcamento.Criar(item.Tipo, item.Descricao, item.Quantidade, item.ValorUnitario, item.EstoqueItemId))
                .ToList();

            var orcamento = Orcamento.Criar(ordem.Id, 1, itens);
            ordem.AguardarAprovacao(orcamento.Id, orcamento.Versao);

            await orcamentoRepository.AdicionarAsync(orcamento, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new CriarOrdemServicoResult(ordem.Id);
    }
}
