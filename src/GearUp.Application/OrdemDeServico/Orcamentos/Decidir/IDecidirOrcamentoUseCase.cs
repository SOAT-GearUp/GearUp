namespace GearUp.Application.OrdemDeServico.Orcamentos.Decidir;

public interface IDecidirOrcamentoUseCase
{
    Task DecidirAsync(DecidirOrcamentoCommand command, CancellationToken ct);
}
