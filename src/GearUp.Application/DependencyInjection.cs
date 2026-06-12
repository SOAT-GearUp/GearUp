using GearUp.Application.Clientes.Cadastrar;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICadastrarClienteUseCase, CadastrarClienteUseCase>();

        return services;
    }
}
