using System.Reflection;

namespace GearUp.Api.HealthChecks;

public static class VersaoAplicacao
{
    public static string Atual { get; } = ObterVersao();

    private static string ObterVersao()
    {
        var assembly = typeof(VersaoAplicacao).Assembly;

        var informacional = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informacional))
            return assembly.GetName().Version?.ToString() ?? "desconhecida";

        // O SourceLink acrescenta "+<hash do commit>" à versão informacional.
        var separador = informacional.IndexOf('+');
        return separador < 0 ? informacional : informacional[..separador];
    }
}
