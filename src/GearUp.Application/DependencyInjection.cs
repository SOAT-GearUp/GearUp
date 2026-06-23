using GearUp.Application.Autenticacao.Autenticar;
using GearUp.Application.Autenticacao.GerenciarUsuarios;
using GearUp.Application.Clientes.Atualizar;
using GearUp.Application.Clientes.Cadastrar;
using GearUp.Application.Clientes.Consultar;
using GearUp.Application.Clientes.Excluir;
using GearUp.Application.Clientes.Listar;
using GearUp.Application.Clientes.Veiculos.Atualizar;
using GearUp.Application.Clientes.Veiculos.Cadastrar;
using GearUp.Application.Common.DomainEvents;
using GearUp.Application.Estoque.Cadastrar;
using GearUp.Application.Estoque.Listar;
using GearUp.Application.Estoque.Movimentar;
using GearUp.Application.Notificacoes.EventHandlers;
using GearUp.Application.Notificacoes.Listar;
using GearUp.Application.OrdensServico.Consultar;
using GearUp.Application.OrdensServico.Criar;
using GearUp.Application.OrdensServico.Diagnosticos.Iniciar;
using GearUp.Application.OrdensServico.Diagnosticos.Registrar;
using GearUp.Application.OrdensServico.Listar;
using GearUp.Application.OrdensServico.Metricas.ObterTempoMedioExecucao;
using GearUp.Application.OrdensServico.Orcamentos.Criar;
using GearUp.Application.OrdensServico.Orcamentos.Decidir;
using GearUp.Application.OrdensServico.Orcamentos.Itens.Adicionar;
using GearUp.Application.OrdensServico.Orcamentos.Itens.Atualizar;
using GearUp.Application.OrdensServico.Orcamentos.Itens.Remover;
using GearUp.Application.OrdensServico.Status.Alterar;
using GearUp.Domain.DomainEvents.Notificacoes;
using GearUp.Domain.DomainEvents.OrdensServico;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        /* Autenticação */
        services.AddScoped<IAutenticarUsuarioUseCase, AutenticarUsuarioUseCase>();
        services.AddScoped<IGerenciarUsuariosUseCase, GerenciarUsuariosUseCase>();
        /* Clientes */
        services.AddScoped<ICadastrarClienteUseCase, CadastrarClienteUseCase>();
        services.AddScoped<IAtualizarClienteUseCase, AtualizarClienteUseCase>();
        services.AddScoped<IConsultarClienteUseCase, ConsultarClienteUseCase>();
        services.AddScoped<IExcluirClienteUseCase, ExcluirClienteUseCase>();
        services.AddScoped<IListarClienteUseCase, ListarClienteUseCase>();
        services.AddScoped<IAtualizarVeiculoUseCase, AtualizarVeiculoUseCase>();
        services.AddScoped<ICadastrarVeiculoUseCase, CadastrarVeiculoUseCase>();
        /* Estoque */
        services.AddScoped<ICadastrarEstoqueItemUseCase, CadastrarEstoqueItemUseCase>();
        services.AddScoped<IListarEstoqueItemUseCase, ListarEstoqueItemUseCase>();
        services.AddScoped<IMovimentarEstoqueItemUseCase, MovimentarEstoqueItemUseCase>();        
        /* Ordens de Serviço */        
        services.AddScoped<ICriarOrdemServicoUseCase, CriarOrdemServicoUseCase>();
        services.AddScoped<IListarOrdemServicoUseCase, ListarOrdemServicoUseCase>();
        services.AddScoped<IConsultarOrdemServicoUseCase, ConsultarOrdemServicoUseCase>();
        services.AddScoped<IIniciarDiagnosticoUseCase, IniciarDiagnosticoUseCase>();
        services.AddScoped<IRegistrarDiagnosticoUseCase, RegistrarDiagnosticoUseCase>();
        services.AddScoped<IAlterarStatusUseCase, AlterarStatusUseCase>();
        /* Metricas */
        services.AddScoped<IObterTempoMedioExecucaoUseCase, ObterTempoMedioExecucaoUseCase>();
        /* Orçamento */
        services.AddScoped<ICriarOrcamentoUseCase, CriarOrcamentoUseCase>();
        services.AddScoped<IDecidirOrcamentoUseCase, DecidirOrcamentoUseCase>();
        services.AddScoped<IAdicionarItemOrcamentoUseCase, AdicionarItemOrcamentoUseCase>();
        services.AddScoped<IAtualizarItemOrcamentoUseCase, AtualizarItemOrcamentoUseCase>();
        services.AddScoped<IRemoverItemOrcamentoUseCase, RemoverItemOrcamentoUseCase>();
        /* Notificações */
        services.AddScoped<IListarNotificacaoUseCase, ListarNotificacaoUseCase>();
        services.AddScoped<IDomainEventHandler<OrcamentoDisponivelDomainEvent>, OrcamentoDisponivelDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<NotificacaoSolicitadaDomainEvent>, NotificacaoSolicitadaDomainEventHandler>();

        return services;
    }
}
