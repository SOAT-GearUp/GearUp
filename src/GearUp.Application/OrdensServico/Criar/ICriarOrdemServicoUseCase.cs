namespace GearUp.Application.OrdensServico.Criar
{
    public interface ICriarOrdemServicoUseCase
    {
        Task<CriarOrdemServicoResult> CriarAsync(CriarOrdemServicoCommand command, CancellationToken ct);
    }
}
