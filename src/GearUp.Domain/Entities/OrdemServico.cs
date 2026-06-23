using GearUp.Domain.Common.DomainEvents;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.DomainEvents.Notificacoes;
using GearUp.Domain.DomainEvents.OrdensServico;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Domain.Entities;

public sealed class OrdemServico : AggregateRoot
{
    private readonly List<Orcamento> _orcamentos = [];
    private readonly List<HistoricoOrdemServico> _historico = [];
    private OrdemServico() { }

    private OrdemServico(Guid clienteId, Guid veiculoId, string solicitacaoInicial, PrioridadeOrdemServico prioridade, DateTimeOffset? prazo)
    {
        Id = Guid.NewGuid(); 
        ClienteId = clienteId; 
        VeiculoId = veiculoId;
        SolicitacaoInicial = ValidarTexto(solicitacaoInicial, "A solicitação inicial é obrigatória.");
        Prioridade = prioridade; 
        Prazo = prazo; 
        Status = StatusOrdemServico.Recebida;
        CriadaEm = DateTimeOffset.UtcNow; 
        RegistrarEvento("OS_CRIADA", "Ordem de serviço recebida.");
    }

    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public string SolicitacaoInicial { get; private set; } = string.Empty;
    public string? Diagnostico { get; private set; }
    public Guid? MecanicoId { get; private set; }
    public PrioridadeOrdemServico Prioridade { get; private set; }
    public DateTimeOffset? Prazo { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset? IniciadaEm { get; private set; }
    public DateTimeOffset? FinalizadaEm { get; private set; }
    public IReadOnlyCollection<Orcamento> Orcamentos => _orcamentos.AsReadOnly();
    public IReadOnlyCollection<HistoricoOrdemServico> Historico => _historico.AsReadOnly();

    public static OrdemServico Criar(Guid clienteId, Guid veiculoId, string solicitacao, PrioridadeOrdemServico prioridade, DateTimeOffset? prazo) =>
        new(clienteId, veiculoId, solicitacao, prioridade, prazo);

    public void IniciarDiagnostico(Guid mecanicoId)
    {
        ExigirStatus(StatusOrdemServico.Recebida);
        MecanicoId = mecanicoId; 
        AlterarStatusInterno(StatusOrdemServico.EmDiagnostico, "DIAGNOSTICO_INICIADO", "Diagnóstico iniciado.");
    }

    public void RegistrarDiagnostico(string diagnostico)
    {
        ExigirStatus(StatusOrdemServico.EmDiagnostico);
        Diagnostico = ValidarTexto(diagnostico, "O diagnóstico é obrigatório.");
        AlterarStatusInterno(StatusOrdemServico.AguardandoOrcamento, "DIAGNOSTICO_REGISTRADO", "Diagnóstico registrado.");
        Notificar(DestinatarioNotificacao.Atendente, "Diagnóstico concluído; orçamento pendente.");
    }

    public Orcamento CriarOrcamento(IEnumerable<NovoItemOrcamento> itens)
    {
        if (Status is not (StatusOrdemServico.AguardandoOrcamento or StatusOrdemServico.AguardandoAprovacao))
            throw new RegraNegocioException("STATUS_OS_INVALIDO", "A OS não está aguardando orçamento.");

        var orcamento = Orcamento.Criar(Id, _orcamentos.Count + 1, itens);
        _orcamentos.Add(orcamento);        
        AlterarStatusInterno(StatusOrdemServico.AguardandoAprovacao, "ORCAMENTO_GERADO", $"Orçamento v{orcamento.Versao} gerado.");
        AdicionarDomainEvent(new OrcamentoDisponivelDomainEvent(Id, ClienteId, orcamento.Id, orcamento.Versao, DateTimeOffset.UtcNow));

        return orcamento;
    }

    public void DecidirOrcamento(Guid orcamentoId, bool aprovado)
    {
        ExigirStatus(StatusOrdemServico.AguardandoAprovacao);

        var orcamento = _orcamentos.SingleOrDefault(item => item.Id == orcamentoId)
            ?? throw new RegraNegocioException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

        orcamento.Decidir(aprovado);

        var status = aprovado 
            ? StatusOrdemServico.AguardandoPecasInsumos 
            : StatusOrdemServico.AguardandoOrcamento;

        AlterarStatusInterno(status, aprovado ? "ORCAMENTO_APROVADO" : "ORCAMENTO_REJEITADO", aprovado ? "Orçamento aprovado." : "Orçamento rejeitado.");
        Notificar(DestinatarioNotificacao.Cliente, aprovado ? "Orçamento aprovado." : "Orçamento rejeitado.");
        Notificar(DestinatarioNotificacao.Atendente, aprovado ? "Orçamento aprovado pelo cliente." : "Orçamento rejeitado pelo cliente.");
    }

    public void AlterarStatus(StatusOrdemServico novoStatus)
    {
        if (novoStatus == StatusOrdemServico.Cancelada)
        {
            if (Status is StatusOrdemServico.Finalizada or StatusOrdemServico.Entregue)
                throw new RegraNegocioException("OS_NAO_PODE_SER_CANCELADA", "OS finalizada ou entregue não pode ser cancelada.");

            AlterarStatusInterno(novoStatus, "OS_CANCELADA", "Ordem de serviço cancelada."); 
            return;
        }

        var permitida = (Status, novoStatus) switch
        {
            (StatusOrdemServico.AguardandoPecasInsumos, StatusOrdemServico.AguardandoExecucao) => true,
            (StatusOrdemServico.AguardandoExecucao, StatusOrdemServico.EmExecucao) => Orcamentos.Any(o => o.Status == StatusOrcamento.Aprovado),
            (StatusOrdemServico.EmExecucao, StatusOrdemServico.Finalizada) => true,
            (StatusOrdemServico.Finalizada, StatusOrdemServico.Entregue) => true,
            _ => false
        };
        
        if (!permitida) 
            throw new RegraNegocioException("TRANSICAO_STATUS_INVALIDA", $"Transição de {Status} para {novoStatus} não permitida.");

        if (novoStatus == StatusOrdemServico.EmExecucao) 
        { 
            IniciadaEm = DateTimeOffset.UtcNow; 
            Notificar(DestinatarioNotificacao.Cliente, "Execução dos serviços iniciada."); 
        }

        if (novoStatus == StatusOrdemServico.Finalizada) 
        { 
            FinalizadaEm = DateTimeOffset.UtcNow; 
            Notificar(DestinatarioNotificacao.Cliente, "Serviços finalizados."); 
        }

        AlterarStatusInterno(novoStatus, $"OS_{novoStatus.ToString().ToUpperInvariant()}", $"Status alterado para {novoStatus}.");
    }

    private void ExigirStatus(StatusOrdemServico esperado)
    {
        if (Status != esperado) 
            throw new RegraNegocioException("STATUS_OS_INVALIDO", $"A OS deve estar em {esperado}.");
    }

    private void AlterarStatusInterno(StatusOrdemServico status, string evento, string descricao)
    {
        Status = status; 
        RegistrarEvento(evento, descricao);
    }

    private void RegistrarEvento(string tipo, string descricao) 
        => _historico.Add(HistoricoOrdemServico.Criar(Id, tipo, descricao));

    private void Notificar(DestinatarioNotificacao destinatario, string mensagem)
        => AdicionarDomainEvent(new NotificacaoSolicitadaDomainEvent(Id, ClienteId, destinatario, mensagem, DateTimeOffset.UtcNow));

    private static string ValidarTexto(string texto, string mensagem) 
        => string.IsNullOrWhiteSpace(texto) ? throw new ArgumentException(mensagem) : texto.Trim();

}
