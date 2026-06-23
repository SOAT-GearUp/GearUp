namespace GearUp.Application.OrdensServico.Orcamentos.Criar
{
    public interface ICriarOrcamentoUseCase
    {
        Task<CriarOrcamentoResult> CriarAsync(CriarOrcamentoCommand command, CancellationToken cancellationToken);
    }
}
