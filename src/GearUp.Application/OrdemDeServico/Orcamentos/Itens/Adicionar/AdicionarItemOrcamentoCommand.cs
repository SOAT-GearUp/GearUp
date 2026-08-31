using GearUp.Domain.Enums;

namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Adicionar;

public sealed record AdicionarItemOrcamentoCommand(
    Guid OrdemServicoId,
    Guid OrcamentoId,
    TipoItemOrcamento Tipo,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    Guid? EstoqueItemId);
