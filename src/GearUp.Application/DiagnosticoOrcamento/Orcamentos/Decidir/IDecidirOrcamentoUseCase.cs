namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Decidir;

public interface IDecidirOrcamentoUseCase
{
    Task DecidirAsync(DecidirOrcamentoCommand command, CancellationToken ct);
}
