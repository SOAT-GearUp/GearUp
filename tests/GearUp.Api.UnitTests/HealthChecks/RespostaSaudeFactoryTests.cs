using GearUp.Api.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GearUp.Api.UnitTests.HealthChecks;

public sealed class RespostaSaudeFactoryTests
{
    [Fact]
    public void Criar_ComRelatorioSaudavel_DeveIncluirStatusVersaoEVerificacoes()
    {
        var relatorio = CriarRelatorio(HealthStatus.Healthy, "PostgreSQL disponível.");

        var resposta = RespostaSaudeFactory.Criar(relatorio);

        Assert.Equal("Healthy", resposta.Status);
        Assert.Equal(VersaoAplicacao.Atual, resposta.Versao);
        Assert.Equal(10, resposta.DuracaoMs);

        var verificacao = Assert.Single(resposta.Verificacoes);
        Assert.Equal("postgres", verificacao.Nome);
        Assert.Equal("Healthy", verificacao.Status);
        Assert.Equal("PostgreSQL disponível.", verificacao.Descricao);
    }

    [Fact]
    public void Criar_ComRelatorioIndisponivel_DeveInformarStatusUnhealthy()
    {
        var relatorio = CriarRelatorio(HealthStatus.Unhealthy, "PostgreSQL indisponível.");

        var resposta = RespostaSaudeFactory.Criar(relatorio);

        Assert.Equal("Unhealthy", resposta.Status);
        Assert.False(string.IsNullOrWhiteSpace(resposta.Versao));
    }

    [Theory]
    [InlineData(HealthStatus.Healthy, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.Degraded, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable)]
    public void ObterStatusHttp_DeveMapearStatusDoRelatorio(HealthStatus status, int esperado)
    {
        Assert.Equal(esperado, RespostaSaudeFactory.ObterStatusHttp(status));
    }

    [Fact]
    public void VersaoAtual_DeveSerInformadaSemMetadadosDeCommit()
    {
        Assert.False(string.IsNullOrWhiteSpace(VersaoAplicacao.Atual));
        Assert.DoesNotContain('+', VersaoAplicacao.Atual);
    }

    private static HealthReport CriarRelatorio(HealthStatus status, string descricao) =>
        new(
            new Dictionary<string, HealthReportEntry>
            {
                ["postgres"] = new(
                    status,
                    descricao,
                    TimeSpan.FromMilliseconds(5),
                    exception: null,
                    data: null)
            },
            TimeSpan.FromMilliseconds(10));
}
