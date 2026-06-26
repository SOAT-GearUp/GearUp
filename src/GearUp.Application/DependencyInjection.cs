using GearUp.Application.Cadastro.Clientes.Atualizar;
using GearUp.Application.Cadastro.Clientes.Cadastrar;
using GearUp.Application.Cadastro.Clientes.Consultar;
using GearUp.Application.Cadastro.Clientes.Excluir;
using GearUp.Application.Cadastro.Clientes.Listar;
using GearUp.Application.Cadastro.Clientes.Veiculos.Atualizar;
using GearUp.Application.Cadastro.Clientes.Veiculos.Cadastrar;
using GearUp.Application.OrdemDeServico.Ordens.Consultar;
using GearUp.Application.OrdemDeServico.Ordens.Criar;
using GearUp.Application.OrdemDeServico.Ordens.Listar;
using GearUp.Application.Autenticacao.Autenticar;
using GearUp.Application.Autenticacao.GerenciarUsuarios;
using GearUp.Application.Common.DomainEvents;
using GearUp.Application.OrdemDeServico.Diagnosticos.IniciarDiagnostico;
using GearUp.Application.OrdemDeServico.Orcamentos.Criar;
using GearUp.Application.OrdemDeServico.Orcamentos.Decidir;
using GearUp.Application.OrdemDeServico.Orcamentos.Itens.Adicionar;
using GearUp.Application.OrdemDeServico.Orcamentos.Itens.Atualizar;
using GearUp.Application.OrdemDeServico.Orcamentos.Itens.Remover;
using GearUp.Application.OrdemDeServico.Diagnosticos.RegistrarDiagnostico;
using GearUp.Application.Estoque.Cadastrar;
using GearUp.Application.Estoque.Listar;
using GearUp.Application.Estoque.Movimentar;
using GearUp.Application.OrdemDeServico.Execucao.AlterarStatus;
using GearUp.Application.OrdemDeServico.Execucao.EventHandlers;
using GearUp.Application.OrdemDeServico.Execucao.Metricas;
using GearUp.Application.Comunicacao.EventHandlers;
using GearUp.Application.Comunicacao.Notificacoes;
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
