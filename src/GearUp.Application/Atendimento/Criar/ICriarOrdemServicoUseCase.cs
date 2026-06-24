namespace GearUp.Application.Atendimento.Criar;

public interface ICriarOrdemServicoUseCase
{
    Task<CriarOrdemServicoResult> CriarAsync(CriarOrdemServicoCommand command, CancellationToken ct);
}
