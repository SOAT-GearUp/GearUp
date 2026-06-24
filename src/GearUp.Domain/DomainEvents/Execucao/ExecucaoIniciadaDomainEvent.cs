using GearUp.Domain.Common.DomainEvents;

namespace GearUp.Domain.DomainEvents.Execucao;

public sealed record ItemDeEstoque(Guid EstoqueItemId, decimal Quantidade, string Descricao);

public sealed record ExecucaoIniciadaDomainEvent(
    Guid OrdemServicoId,
    Guid ClienteId,
    IReadOnlyList<ItemDeEstoque> ItensParaDeduzir,
    DateTimeOffset OcorridoEm) : IDomainEvent;
