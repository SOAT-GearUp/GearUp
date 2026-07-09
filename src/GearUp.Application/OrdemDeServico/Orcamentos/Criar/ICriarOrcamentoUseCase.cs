namespace GearUp.Application.OrdemDeServico.Orcamentos.Criar;

public interface ICriarOrcamentoUseCase
{
    Task<CriarOrcamentoResult> CriarAsync(CriarOrcamentoCommand command, CancellationToken ct);
}
