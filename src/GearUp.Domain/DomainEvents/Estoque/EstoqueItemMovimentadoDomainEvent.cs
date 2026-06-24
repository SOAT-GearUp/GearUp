using GearUp.Domain.Common.DomainEvents;
using GearUp.Domain.Enums;

namespace GearUp.Domain.DomainEvents.Estoque;

public sealed record EstoqueItemMovimentadoDomainEvent(
    Guid EstoqueItemId,
    string NomeItem,
    TipoMovimentacaoEstoque TipoMovimentacao,
    decimal Quantidade,
    decimal QuantidadeDisponivel,
    Guid? OrdemServicoId,
    DateTimeOffset OcorridoEm) : IDomainEvent;
