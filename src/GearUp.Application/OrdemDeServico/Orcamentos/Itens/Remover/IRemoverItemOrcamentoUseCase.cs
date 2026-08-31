namespace GearUp.Application.OrdemDeServico.Orcamentos.Itens.Remover;

public interface IRemoverItemOrcamentoUseCase
{
    Task RemoverAsync(RemoverItemOrcamentoCommand command, CancellationToken ct);
}
