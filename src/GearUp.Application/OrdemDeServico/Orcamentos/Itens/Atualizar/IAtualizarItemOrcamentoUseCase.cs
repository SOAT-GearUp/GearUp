namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Atualizar;

public interface IAtualizarItemOrcamentoUseCase
{
    Task AtualizarAsync(AtualizarItemOrcamentoCommand command, CancellationToken ct);
}
