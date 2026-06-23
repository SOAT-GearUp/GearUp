namespace GearUp.Application.OrdensServico.Orcamentos.Criar
{
    public sealed record CriarOrcamentoResult(
        Guid Id,
        int Versao,
        decimal ValorTotal);
}
