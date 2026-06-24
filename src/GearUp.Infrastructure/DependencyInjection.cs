using GearUp.Application.Autenticacao.Common;
using GearUp.Application.Atendimento.Clientes.Common.Interfaces;
using GearUp.Application.Atendimento.Clientes.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Infrastructure.DomainEvents;
using GearUp.Infrastructure.Persistence;
using GearUp.Infrastructure.Persistence.Repositories;
using GearUp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using IAtendimentoRepo = GearUp.Application.Atendimento.Comum.Interfaces.IOrdemServicoRepository;
using IDiagnosticoRepo = GearUp.Application.DiagnosticoOrcamento.Comum.Interfaces.IOrdemServicoRepository;
using IExecucaoRepo = GearUp.Application.Execucao.Comum.Interfaces.IOrdemServicoRepository;
using IAtendimentoOrcamentoRepo = GearUp.Application.Atendimento.Comum.Interfaces.IOrcamentoRepository;
using IDiagnosticoOrcamentoRepo = GearUp.Application.DiagnosticoOrcamento.Orcamentos.Common.Interfaces.IOrcamentoRepository;
using IExecucaoOrcamentoRepo = GearUp.Application.Execucao.Comum.Interfaces.IOrcamentoRepository;

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
        services.AddScoped<IComunicacaoRepository, ComunicacaoRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IUnitOfWork>(
            serviceProvider => serviceProvider.GetRequiredService<GearUpDbContext>());

        return services;
    }
}
