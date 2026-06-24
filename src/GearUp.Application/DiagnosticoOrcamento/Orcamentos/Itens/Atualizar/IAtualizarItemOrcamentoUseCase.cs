namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Atualizar;

public interface IAtualizarItemOrcamentoUseCase
{
    Task AtualizarAsync(AtualizarItemOrcamentoCommand command, CancellationToken ct);
}
