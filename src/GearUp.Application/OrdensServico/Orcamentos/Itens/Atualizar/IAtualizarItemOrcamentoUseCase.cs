using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.OrdensServico.Orcamentos.Itens.Atualizar
{
    public interface IAtualizarItemOrcamentoUseCase
    {
        Task AtualizarAsync(AtualizarItemOrcamentoCommand command, CancellationToken ct);
    }
}
