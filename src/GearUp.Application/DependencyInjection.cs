using GearUp.Application.Atendimento.Clientes.Atualizar;
using GearUp.Application.Atendimento.Clientes.Cadastrar;
using GearUp.Application.Atendimento.Clientes.Consultar;
using GearUp.Application.Atendimento.Clientes.Excluir;
using GearUp.Application.Atendimento.Clientes.Listar;
using GearUp.Application.Atendimento.Clientes.Veiculos.Atualizar;
using GearUp.Application.Atendimento.Clientes.Veiculos.Cadastrar;
using GearUp.Application.Atendimento.Consultar;
using GearUp.Application.Atendimento.Criar;
using GearUp.Application.Atendimento.Listar;
using GearUp.Application.Autenticacao.Autenticar;
using GearUp.Application.Autenticacao.GerenciarUsuarios;
using GearUp.Application.Common.DomainEvents;
using GearUp.Application.DiagnosticoOrcamento.IniciarDiagnostico;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Criar;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Decidir;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Adicionar;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Atualizar;
using GearUp.Application.DiagnosticoOrcamento.Orcamentos.Itens.Remover;
using GearUp.Application.DiagnosticoOrcamento.RegistrarDiagnostico;
using GearUp.Application.Estoque.Cadastrar;
using GearUp.Application.Estoque.Listar;
using GearUp.Application.Estoque.Movimentar;
using GearUp.Application.Execucao.AlterarStatus;
using GearUp.Application.Execucao.EventHandlers;
using GearUp.Application.Execucao.Metricas;
using GearUp.Application.Notificacoes.EventHandlers;
using GearUp.Application.Notificacoes.Listar;
using GearUp.Domain.DomainEvents.DiagnosticoOrcamento;
using GearUp.Domain.DomainEvents.Execucao;
using GearUp.Domain.DomainEvents.Notificacoes;
using Microsoft.Extensions.DependencyInjection;

namespace GearUp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        /* Autenticação */
        services.AddScoped<IAutenticarUsuarioUseCase, AutenticarUsuarioUseCase>();
        services.AddScoped<IGerenciarUsuariosUseCase, GerenciarUsuariosUseCase>();
        /* Atendimento — Clientes */
        services.AddScoped<ICadastrarClienteUseCase, CadastrarClienteUseCase>();
        services.AddScoped<IAtualizarClienteUseCase, AtualizarClienteUseCase>();
        services.AddScoped<IConsultarClienteUseCase, ConsultarClienteUseCase>();
        services.AddScoped<IExcluirClienteUseCase, ExcluirClienteUseCase>();
        services.AddScoped<IListarClienteUseCase, ListarClienteUseCase>();
        services.AddScoped<IAtualizarVeiculoUseCase, AtualizarVeiculoUseCase>();
        services.AddScoped<ICadastrarVeiculoUseCase, CadastrarVeiculoUseCase>();
        /* Atendimento — Ordens de Serviço */
        services.AddScoped<ICriarOrdemServicoUseCase, CriarOrdemServicoUseCase>();
        services.AddScoped<IListarOrdemServicoUseCase, ListarOrdemServicoUseCase>();
        services.AddScoped<IConsultarOrdemServicoUseCase, ConsultarOrdemServicoUseCase>();
        /* Diagnóstico & Orçamento */
        services.AddScoped<IIniciarDiagnosticoUseCase, IniciarDiagnosticoUseCase>();
        services.AddScoped<IRegistrarDiagnosticoUseCase, RegistrarDiagnosticoUseCase>();
        services.AddScoped<ICriarOrcamentoUseCase, CriarOrcamentoUseCase>();
        services.AddScoped<IDecidirOrcamentoUseCase, DecidirOrcamentoUseCase>();
        services.AddScoped<IAdicionarItemOrcamentoUseCase, AdicionarItemOrcamentoUseCase>();
        services.AddScoped<IAtualizarItemOrcamentoUseCase, AtualizarItemOrcamentoUseCase>();
        services.AddScoped<IRemoverItemOrcamentoUseCase, RemoverItemOrcamentoUseCase>();
        /* Execução */
        services.AddScoped<IAlterarStatusUseCase, AlterarStatusUseCase>();
        services.AddScoped<IObterTempoMedioExecucaoUseCase, ObterTempoMedioExecucaoUseCase>();
        /* Estoque */
        services.AddScoped<ICadastrarEstoqueItemUseCase, CadastrarEstoqueItemUseCase>();
        services.AddScoped<IListarEstoqueItemUseCase, ListarEstoqueItemUseCase>();
        services.AddScoped<IMovimentarEstoqueItemUseCase, MovimentarEstoqueItemUseCase>();
        /* Notificações e Event Handlers */
        services.AddScoped<IListarNotificacaoUseCase, ListarNotificacaoUseCase>();
        services.AddScoped<IDomainEventHandler<ExecucaoIniciadaDomainEvent>, ExecucaoIniciadaDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<OrcamentoDisponivelDomainEvent>, OrcamentoDisponivelDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<NotificacaoSolicitadaDomainEvent>, NotificacaoSolicitadaDomainEventHandler>();

        return services;
    }
}
