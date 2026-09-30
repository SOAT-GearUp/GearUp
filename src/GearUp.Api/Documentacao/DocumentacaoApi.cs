namespace GearUp.Api.Documentacao;

internal static class DocumentacaoApi
{
    public const string ChaveHabilitada = "Swagger:Habilitado";

    // Swagger sempre ligado em Development e, fora dele, só quando o ambiente
    // pede explicitamente (homologação, via Swagger__Habilitado=true). Em
    // produção fica desligado: a documentação publicada é a de homologação.
    public static bool Habilitada(IHostEnvironment ambiente, IConfiguration configuracao) =>
        ambiente.IsDevelopment() || configuracao.GetValue<bool>(ChaveHabilitada);
}
