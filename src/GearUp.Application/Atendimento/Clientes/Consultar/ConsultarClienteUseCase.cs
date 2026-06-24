using GearUp.Application.Atendimento.Clientes.Common;
using GearUp.Application.Atendimento.Clientes.Common.Interfaces;
using GearUp.Application.Atendimento.Clientes.Veiculos.Common.Interfaces;

namespace GearUp.Application.Atendimento.Clientes.Consultar;

internal sealed class ConsultarClienteUseCase(
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository) : IConsultarClienteUseCase
{
    public async Task<ConsultarClienteResult> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await clienteRepository.ObterAsync(id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");

        var veiculos = await veiculoRepository.ListarPorClienteAsync(cliente.Id, cancellationToken);

        return new ConsultarClienteResult(
            Id: cliente.Id,
            Nome: cliente.Nome,
            Documento: cliente.Documento.ToString(),
            TipoDocumento: cliente.Documento.Tipo.ToString(),
            Email: cliente.Email.ToString(),
            Telefone: cliente.Telefone.ToString(),
            Veiculos: veiculos.Select(v => new VeiculoResult(v.Id, v.Placa, v.Marca, v.Modelo, v.Ano)).ToList()
        );
    }
}
