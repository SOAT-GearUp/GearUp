namespace GearUp.Api.Contracts.Saude
{
    public sealed record SaudeResponse(
        string Status,
        string Versao,
        double DuracaoMs,
        IReadOnlyCollection<VerificacaoSaudeResponse> Verificacoes);

    public sealed record VerificacaoSaudeResponse(
        string Nome,
        string Status,
        string? Descricao);
}
