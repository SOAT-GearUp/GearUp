namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Remover;

public interface IRemoverItemOrcamentoUseCase
{
    Task RemoverAsync(RemoverItemOrcamentoCommand command, CancellationToken ct);
}
