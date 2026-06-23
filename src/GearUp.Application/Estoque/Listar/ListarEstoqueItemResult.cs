using System;
using System.Collections.Generic;
using System.Text;

namespace GearUp.Application.Estoque.Listar
{
    public sealed record ListarEstoqueItemResult(
        Guid Id,
        string Nome,
        string Descricao,
        decimal QuantidadeDisponivel,
        decimal PrecoUnitario);
}
