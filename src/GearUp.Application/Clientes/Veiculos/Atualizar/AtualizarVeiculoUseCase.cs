using GearUp.Application.Clientes.Common.Interfaces;
using GearUp.Application.Common.Interfaces;

namespace GearUp.Application.Clientes.Veiculos.Atualizar
{
    internal class AtualizarVeiculoUseCase(IClienteRepository clienteRepository, IUnitOfWork unitOfWork) : IAtualizarVeiculoUseCase
    {
        public async Task AtualizarVeiculoAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken)
        {
            var veiculo = await clienteRepository.ObterVeiculoAsync(command.VeiculoId, cancellationToken) 
                ?? throw new RecursoNaoEncontradoException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");

            if (veiculo.ClienteId != command.ClienteId)
                throw new RecursoNaoEncontradoException(
                    "VEICULO_NAO_ENCONTRADO",
                    "Veículo não encontrado para o cliente informado.");

            if (await clienteRepository.PlacaExisteAsync(command.Placa, command.VeiculoId, cancellationToken)) 
                throw new ConflitoException("PLACA_DUPLICADA", "Já existe veículo cadastrado com essa placa.");

            veiculo.Atualizar(command.Placa, command.Marca, command.Modelo, command.Ano); 
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
