using GearUp.Application.Autenticacao.Common;
using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Cadastro.Veiculos.Common.Interfaces;
using GearUp.Application.Common.Interfaces;
using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Infrastructure.Persistence;
using GearUp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using IAtendimentoOrcamentoRepository = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrcamentoRepository;
using IAtendimentoOrdemServicoRepository = GearUp.Application.OrdemDeServico.Common.Interfaces.IOrdemServicoRepository;
using IDiagnosticoOrcamentoRepository = GearUp.Application.OrdemDeServico.Orcamentos.Common.Interfaces.IOrcamentoRepository;
using IDiagnosticoOrdemServicoRepository = GearUp.Application.OrdemDeServico.Diagnosticos.Common.Interfaces.IOrdemServicoRepository;
using IExecucaoOrcamentoRepository = GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces.IOrcamentoRepository;
using IExecucaoOrdemServicoRepository = GearUp.Application.OrdemDeServico.Execucao.Common.Interfaces.IOrdemServicoRepository;

namespace GearUp.Infrastructure.UnitTests.Persistence;

internal sealed class InMemoryDbContextFactory : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public InMemoryDbContextFactory()
    {
        var services = new ServiceCollection();

        var databaseName = Guid.NewGuid().ToString();

        services.AddDbContext<GearUpDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEstoqueRepository, EstoqueRepository>();
        services.AddScoped<IAtendimentoOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IDiagnosticoOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IExecucaoOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IAtendimentoOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<IDiagnosticoOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<IExecucaoOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<GearUpDbContext>());

        _serviceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }
}
