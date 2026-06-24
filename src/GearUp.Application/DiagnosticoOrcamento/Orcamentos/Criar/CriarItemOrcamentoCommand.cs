using GearUp.Domain.Enums;

namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Criar;

public sealed record CriarItemOrcamentoCommand(
    TipoItemOrcamento Tipo,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    Guid? EstoqueItemId);
