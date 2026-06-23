namespace GearUp.Application.OrdensServico.Orcamentos.Decidir
{
    public interface IDecidirOrcamentoUseCase
    {
        Task DecidirAsync(DecidirOrcamentoCommand command, CancellationToken ct);
    }
}
