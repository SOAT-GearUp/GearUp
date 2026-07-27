using GearUp.Application.Autenticacao.Common.Interfaces;
using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Infrastructure.DomainEvents;
using GearUp.Infrastructure.Persistence;
using GearUp.Infrastructure.Persistence.Repositories;
using GearUp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using IAtendimentoRepo = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrdemServicoRepository;
using IDiagnosticoRepo = GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces.IOrdemServicoRepository;
using IExecucaoRepo = GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces.IOrdemServicoRepository;
using IAtendimentoOrcamentoRepo = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrcamentoRepository;
using IDiagnosticoOrcamentoRepo = GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces.IOrcamentoRepository;
using IExecucaoOrcamentoRepo = GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces.IOrcamentoRepository;

namespace GearUp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GearUpDatabase")
            ?? throw new InvalidOperationException(
                "A connection string 'GearUpDatabase' não foi configurada.");

        services.AddDbContext<GearUpDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEstoqueRepository, EstoqueRepository>();
        services.AddScoped<IAtendimentoRepo, OrdemServicoRepository>();
        services.AddScoped<IDiagnosticoRepo, OrdemServicoRepository>();
        services.AddScoped<IExecucaoRepo, OrdemServicoRepository>();
        services.AddScoped<IAtendimentoOrcamentoRepo, OrcamentoRepository>();
        services.AddScoped<IDiagnosticoOrcamentoRepo, OrcamentoRepository>();
        services.AddScoped<IExecucaoOrcamentoRepo, OrcamentoRepository>();
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IUnitOfWork>(
            serviceProvider => serviceProvider.GetRequiredService<GearUpDbContext>());

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"]);

        return services;
    }
}
