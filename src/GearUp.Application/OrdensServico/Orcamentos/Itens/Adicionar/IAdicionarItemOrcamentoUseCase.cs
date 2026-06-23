namespace GearUp.Application.OrdensServico.Orcamentos.Itens.Adicionar
{
    public interface IAdicionarItemOrcamentoUseCase
    {
        Task AdicionarAsync(AdicionarItemOrcamentoCommand command, CancellationToken ct);
    }
}
