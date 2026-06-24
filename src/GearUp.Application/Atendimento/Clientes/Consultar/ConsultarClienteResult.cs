using GearUp.Application.Atendimento.Clientes.Common;

namespace GearUp.Application.Atendimento.Clientes.Consultar;

public sealed record ConsultarClienteResult(
    Guid Id,
    string Nome,
    string Documento,
    string TipoDocumento,
    string Email,
    string Telefone,
    IReadOnlyCollection<VeiculoResult> Veiculos);
