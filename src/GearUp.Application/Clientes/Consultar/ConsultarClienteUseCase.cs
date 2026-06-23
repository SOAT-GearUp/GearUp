using GearUp.Application.Clientes.Common;
using GearUp.Application.Clientes.Common.Interfaces;
using GearUp.Application.Common;

namespace GearUp.Application.Clientes.Consultar
{
    internal sealed class ConsultarClienteUseCase(IClienteRepository clienteRepository) : IConsultarClienteUseCase
    {
        public async Task<ConsultarClienteResult> ObterAsync(Guid id, CancellationToken cancellationToken)
        {    
            var cliente = await clienteRepository.ObterAsync(id, cancellationToken) 
                ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

            return new ConsultarClienteResult(
                Id: cliente.Id,
                Nome: cliente.Nome,
                Documento: cliente.Documento.ToString(),
                TipoDocumento: cliente.Documento.Tipo.ToString(),
                Email: cliente.Email.ToString(),
                Telefone: cliente.Telefone.ToString(),
                Veiculos: cliente.Veiculos.Select(v => new VeiculoResult(
                    Id: v.Id,
                    Placa: v.Placa,
                    Marca: v.Marca,
                    Modelo: v.Modelo,
                    Ano: v.Ano
                )).ToList()
            );
        }
    }
}
