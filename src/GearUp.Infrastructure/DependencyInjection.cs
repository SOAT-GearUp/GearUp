using GearUp.Application.Clientes;
using GearUp.Application.Common;
using GearUp.Infrastructure.Persistence;
using GearUp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            options.UseSqlServer(connectionString));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IUnitOfWork>(
            serviceProvider => serviceProvider.GetRequiredService<GearUpDbContext>());

        return services;
    }
}
