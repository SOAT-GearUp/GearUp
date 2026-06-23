using GearUp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearUp.Application.Estoque.Listar
{
    public interface IListarEstoqueItemUseCase
    {
        Task<IReadOnlyList<ListarEstoqueItemResult>> ListarAsync(CancellationToken ct);
    }
}
