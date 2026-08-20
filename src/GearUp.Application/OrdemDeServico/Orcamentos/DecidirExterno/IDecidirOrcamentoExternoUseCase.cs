namespace GearUp.Application.OrdemDeServico.Orcamentos.DecidirExterno;

public interface IDecidirOrcamentoExternoUseCase
{
    Task DecidirAsync(DecidirOrcamentoExternoCommand command, CancellationToken ct);
}
