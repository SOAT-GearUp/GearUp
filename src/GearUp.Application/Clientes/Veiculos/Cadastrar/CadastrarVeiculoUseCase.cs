using GearUp.Application.Clientes.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GearUp.Application.Clientes.Veiculos.Cadastrar
{
    internal sealed class CadastrarVeiculoUseCase(IClienteRepository clienteRepository, IUnitOfWork unitOfWork) : ICadastrarVeiculoUseCase
    {
        public async Task<CadastrarVeiculoResult> CadastrarVeiculoAsync(CadastrarVeiculoCommand command, CancellationToken cancellationToken)
        {
            if (await clienteRepository.PlacaExisteAsync(command.Placa, null, cancellationToken)) 
                throw new ConflitoException("PLACA_DUPLICADA", "Já existe veículo cadastrado com essa placa.");

            var cliente = await clienteRepository.ObterAsync(command.ClienteId, cancellationToken)
                ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

            var veiculo = cliente.AdicionarVeiculo(
                command.Placa,
                command.Marca,
                command.Modelo,
                command.Ano);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CadastrarVeiculoResult(veiculo.Id);
        }
    }
}
