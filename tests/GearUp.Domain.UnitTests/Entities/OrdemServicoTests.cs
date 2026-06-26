using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.DomainEvents.Notificacoes;
using GearUp.Domain.DomainEvents.DiagnosticoOrcamento;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class OrdemServicoTests
{
    [Fact]
    public void Criar_DeveIniciarRecebidaERegistrarHistorico()
    {
        var os = Criar();

        Assert.Equal(StatusOrdemServico.Recebida, os.Status);
        Assert.Contains(os.Historico, evento => evento.Tipo == "OS_CRIADA");
    }

    [Fact]
    public void FluxoDiagnosticoEOrcamentoAprovado_DeveAguardarPecas()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Trocar pastilhas.");

        var orcamentoId = Guid.NewGuid();
        os.AguardarAprovacao(orcamentoId, 1);
        os.ReceberDecisaoOrcamento(orcamentoId, true);

        Assert.Equal(StatusOrdemServico.AguardandoPecasInsumos, os.Status);
        Assert.Contains(os.DomainEvents.OfType<OrcamentoDisponivelDomainEvent>(), e => e.Versao == 1);
        Assert.Contains(os.DomainEvents.OfType<NotificacaoSolicitadaDomainEvent>(), e => e.Destinatario == DestinatarioNotificacao.Cliente);
    }

    [Fact]
    public void TransicaoStatusInvalida_DeveFalhar()
    {
        var os = Criar();

        Assert.Throws<RegraNegocioException>(() => os.AlterarStatus(StatusOrdemServico.EmExecucao));
    }

    [Fact]
    public void OrcamentoDecidido_NaoPodeSerAlterado()
    {
        var orcamento = CriarOrcamento([Item(TipoItemOrcamento.Servico, "Alinhamento", 1, 100, null)]);
        orcamento.Decidir(true);

        Assert.Throws<RegraNegocioException>(() => orcamento.AdicionarItem(Item(TipoItemOrcamento.Servico, "Extra", 1, 50, null)));
    }

    [Fact]
    public void OrcamentoPendente_DevePermitirManutencaoDeItens()
    {
        var orcamento = CriarOrcamento([Item(TipoItemOrcamento.Servico, "Alinhamento", 1, 100, null)]);

        var novo = orcamento.AdicionarItem(Item(TipoItemOrcamento.MaoDeObra, "Mao de obra", 2, 50, null));
        orcamento.AtualizarItem(novo.Id, Item(TipoItemOrcamento.MaoDeObra, "Mao de obra especializada", 3, 60, null));

        Assert.Equal(280, orcamento.ValorTotal);

        orcamento.RemoverItem(novo.Id);

        Assert.Single(orcamento.Itens);
    }

    [Fact]
    public void RejeitarOrcamento_DevePermitirNovaVersao()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnostico");

        var primeiroId = Guid.NewGuid();
        os.AguardarAprovacao(primeiroId, 1);
        os.ReceberDecisaoOrcamento(primeiroId, false);

        var segundoId = Guid.NewGuid();
        os.AguardarAprovacao(segundoId, 2);

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
    }

    [Fact]
    public void FluxoAprovado_DeveExecutarFinalizarEEntregar()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnostico");

        var orcamentoId = Guid.NewGuid();
        os.AguardarAprovacao(orcamentoId, 1);
        os.ReceberDecisaoOrcamento(orcamentoId, true);
        os.AlterarStatus(StatusOrdemServico.AguardandoExecucao);
        os.IniciarExecucao([]);
        os.AlterarStatus(StatusOrdemServico.Finalizada);
        os.AlterarStatus(StatusOrdemServico.Entregue);

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
        Assert.NotNull(os.IniciadaEm);
        Assert.NotNull(os.FinalizadaEm);
    }

    [Fact]
    public void Notificacao_DeveSerMarcadaComoLida()
    {
        var notificacao = Notificacao.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DestinatarioNotificacao.Cliente,
            "Mensagem");

        notificacao.MarcarComoLida();
        notificacao.MarcarComoLida();

        Assert.NotNull(notificacao.LidaEm);
    }

    private static OrdemServico Criar()
    {
        return OrdemServico.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ruido nos freios",
            PrioridadeOrdemServico.Normal,
            null);
    }

    private static Orcamento CriarOrcamento(IEnumerable<NovoItemOrcamento> itens)
    {
        return Orcamento.Criar(Guid.NewGuid(), 1, itens);
    }

    private static NovoItemOrcamento Item(
        TipoItemOrcamento tipo,
        string descricao,
        decimal quantidade,
        decimal valorUnitario,
        Guid? estoqueItemId)
    {
        return NovoItemOrcamento.Criar(tipo, descricao, quantidade, valorUnitario, estoqueItemId);
    }
}
