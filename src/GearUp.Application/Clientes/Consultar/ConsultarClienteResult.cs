using System;
using System.Collections.Generic;
using GearUp.Application.Clientes.Common;

namespace GearUp.Application.Clientes.Consultar
{
    public sealed record ConsultarClienteResult(
        Guid Id,
        string Nome,
        string Documento,
        string TipoDocumento,
        string Email,
        string Telefone,
        IReadOnlyCollection<VeiculoResult> Veiculos);
}
