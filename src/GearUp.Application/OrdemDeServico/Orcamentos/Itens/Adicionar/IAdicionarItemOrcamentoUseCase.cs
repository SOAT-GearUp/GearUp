namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Adicionar;

public interface IAdicionarItemOrcamentoUseCase
{
    Task AdicionarAsync(AdicionarItemOrcamentoCommand command, CancellationToken ct);
}
