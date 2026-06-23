namespace GearUp.Application.OrdensServico.Orcamentos.Itens.Remover
{
    public interface IRemoverItemOrcamentoUseCase
    {
        Task RemoverAsync(RemoverItemOrcamentoCommand command, CancellationToken ct);
    }
}
