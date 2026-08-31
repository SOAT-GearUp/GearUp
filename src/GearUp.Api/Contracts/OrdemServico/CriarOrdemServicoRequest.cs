using GearUp.Api.Contracts.Orcamentos;
using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.OrdemServico
{
    public sealed record CriarOrdemServicoRequest(
        [Required] Guid ClienteId, 
        [Required] Guid VeiculoId, 
        [Required] string SolicitacaoInicial, 
        [Required] PrioridadeOrdemServico Prioridade, 
        DateTimeOffset? Prazo,
        IReadOnlyCollection<ItemOrcamentoRequest>? Itens);
}
