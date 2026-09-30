using GearUp.Api.Documentacao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace GearUp.Api.UnitTests.Documentacao;

public sealed class DocumentacaoApiTests
{
    private sealed class AmbienteFake(string nome) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = nome;
        public string ApplicationName { get; set; } = "GearUp.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static IConfiguration Configuracao(string? habilitado) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [DocumentacaoApi.ChaveHabilitada] = habilitado })
            .Build();

    [Fact]
    public void Habilitada_EmDevelopment_DeveSerVerdadeiroMesmoSemConfiguracao()
    {
        Assert.True(DocumentacaoApi.Habilitada(new AmbienteFake(Environments.Development), Configuracao(null)));
    }

    [Fact]
    public void Habilitada_EmHomologComFlag_DeveSerVerdadeiro()
    {
        Assert.True(DocumentacaoApi.Habilitada(new AmbienteFake("Homolog"), Configuracao("true")));
    }

    [Fact]
    public void Habilitada_EmProducaoSemFlag_DeveSerFalso()
    {
        Assert.False(DocumentacaoApi.Habilitada(new AmbienteFake(Environments.Production), Configuracao(null)));
    }

    [Fact]
    public void Habilitada_EmProducaoComFlagDesligada_DeveSerFalso()
    {
        Assert.False(DocumentacaoApi.Habilitada(new AmbienteFake(Environments.Production), Configuracao("false")));
    }
}
