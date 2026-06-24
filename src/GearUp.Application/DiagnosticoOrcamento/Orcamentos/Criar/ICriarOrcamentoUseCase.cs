namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Criar;

public interface ICriarOrcamentoUseCase
{
    Task<CriarOrcamentoResult> CriarAsync(CriarOrcamentoCommand command, CancellationToken ct);
}
