namespace GearUp.Application.OrdemDeServico.Ordens.Criar;

public interface ICriarOrdemServicoUseCase
{
    Task<CriarOrdemServicoResult> CriarAsync(CriarOrdemServicoCommand command, CancellationToken ct);
}
