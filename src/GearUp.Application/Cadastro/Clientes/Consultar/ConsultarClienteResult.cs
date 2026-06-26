using GearUp.Application.Cadastro.Clientes.Common;

namespace GearUp.Application.Cadastro.Clientes.Consultar;

public sealed record ConsultarClienteResult(
    Guid Id,
    string Nome,
    string Documento,
    string TipoDocumento,
    string Email,
    string Telefone,
    IReadOnlyCollection<VeiculoResult> Veiculos);
