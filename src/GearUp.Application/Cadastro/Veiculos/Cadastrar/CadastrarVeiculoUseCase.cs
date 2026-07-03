using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.Cadastro.Veiculos.Cadastrar;

internal sealed class CadastrarVeiculoUseCase(
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IUnitOfWork unitOfWork) : ICadastrarVeiculoUseCase
{
    public async Task<CadastrarVeiculoResult> CadastrarVeiculoAsync(CadastrarVeiculoCommand command, CancellationToken cancellationToken)
    {
        if (await veiculoRepository.PlacaExisteAsync(command.Placa, null, cancellationToken))
            throw new ConflitoException("PLACA_DUPLICADA", "Já existe veículo cadastrado com essa placa.");

        var cliente = await clienteRepository.ObterAsync(command.ClienteId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

        var veiculo = Veiculo.Criar(cliente.Id, command.Placa, command.Marca, command.Modelo, command.Ano);

        await veiculoRepository.AdicionarAsync(veiculo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CadastrarVeiculoResult(veiculo.Id);
    }
}
