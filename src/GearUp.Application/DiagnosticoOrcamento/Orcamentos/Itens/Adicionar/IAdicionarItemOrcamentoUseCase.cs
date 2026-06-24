namespace GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Adicionar;

public interface IAdicionarItemOrcamentoUseCase
{
    Task AdicionarAsync(AdicionarItemOrcamentoCommand command, CancellationToken ct);
}
