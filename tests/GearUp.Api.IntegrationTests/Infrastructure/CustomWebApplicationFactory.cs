using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GearUp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace GearUp.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Fábrica de aplicação para testes de integração. Sobe um container PostgreSQL
/// dedicado via Testcontainers e injeta a configuração necessária (JWT e seed)
/// para que o <c>DatabaseInitializer</c> real crie o schema e o usuário admin
/// inicial. Os demais usuários operacionais são criados via API no bootstrap
/// dos testes.
/// </summary>
/// <remarks>
/// Cada classe de teste recebe sua própria instância (via <c>IClassFixture</c>) e,
/// portanto, seu próprio container — garantindo isolamento total entre classes.
/// </remarks>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Senha do admin de seed e dos usuários operacionais criados no bootstrap.
    /// </summary>
    public const string SenhaSeed = "Teste@Integracao123";

    private const string JwtKey = "chave-de-teste-de-integracao-com-mais-de-32-chars";
    private const string JwtIssuer = "GearUp.IntegrationTests";
    private const string JwtAudience = "GearUp.IntegrationTests";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private readonly SemaphoreSlim _bootstrapLock = new(1, 1);
    private bool _usuariosOperacionaisGarantidos;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly (string Usuario, int Perfil)[] UsuariosOperacionais =
    [
        ("atendente", 1),
        ("auxiliar", 2),
        ("mecanico", 3),
    ];

    public async Task GarantirUsuariosOperacionaisAsync()
    {
        if (_usuariosOperacionaisGarantidos)
            return;

        await _bootstrapLock.WaitAsync();
        try
        {
            if (_usuariosOperacionaisGarantidos)
                return;

            var admin = await AutenticarAsync("admin");

            foreach (var (usuario, perfil) in UsuariosOperacionais)
            {
                var resposta = await admin.PostAsJsonAsync(
                    "/api/usuarios",
                    new
                    {
                        Usuario = usuario,
                        Senha = SenhaSeed,
                        Perfil = perfil,
                        ClienteId = (Guid?)null,
                    },
                    JsonOptions);

                if (resposta.StatusCode is not System.Net.HttpStatusCode.Created
                    and not System.Net.HttpStatusCode.Conflict)
                {
                    resposta.EnsureSuccessStatusCode();
                }
            }

            _usuariosOperacionaisGarantidos = true;
        }
        finally
        {
            _bootstrapLock.Release();
        }
    }

    public async Task<HttpClient> AutenticarAsync(string usuario)
    {
        var client = CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/api/autenticacao/login",
            new { Usuario = usuario, Senha = SenhaSeed },
            JsonOptions);

        resposta.EnsureSuccessStatusCode();

        var token = await resposta.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Resposta de login sem corpo válido.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return client;
    }

    private sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiraEm);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,

                ["Seed:AdminUser"] = "admin",
                ["Seed:AdminPassword"] = SenhaSeed,
            });
        });

        // Program.cs captura Jwt:Key e a connection string no registro dos serviços,
        // antes dos callbacks de ConfigureAppConfiguration. Por isso sobrescrevemos
        // DbContext e JwtBearer aqui, apontando tudo ao container e às credenciais
        // de teste — isolado por factory, sem estado global.
        builder.ConfigureTestServices(services =>
        {
            RemoverRegistrosDbContext(services);

            services.AddDbContext<GearUpDbContext>(options =>
                options.UseNpgsql(
                    _container.GetConnectionString(),
                    npgsql => npgsql.EnableRetryOnFailure()));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // O TokenService emite "role" no JWT; no .NET 8+ o bearer não remapeia
                // claims de entrada por padrão, e [Authorize(Roles=...)] retorna 401.
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        });
    }

    private static void RemoverRegistrosDbContext(IServiceCollection services)
    {
        var descritores = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<GearUpDbContext>)
                || d.ServiceType == typeof(DbContextOptions)
                || d.ServiceType == typeof(GearUpDbContext)
                // IDbContextOptionsConfiguration<TContext> é registrado pelo AddDbContext
                // no EF Core 9+; comparamos por nome para não depender do tipo público.
                || d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal))
            .ToList();

        foreach (var descritor in descritores)
            services.Remove(descritor);
    }

    async Task IAsyncLifetime.InitializeAsync() => await _container.StartAsync();

    async Task IAsyncLifetime.DisposeAsync() => await _container.DisposeAsync();
}
