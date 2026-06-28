using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.DomainEvents.Execucao;
using GearUp.Domain.DomainEvents.DiagnosticoOrcamento;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class OrdemServicoAdicionaisTests
{
    private static OrdemServico Criar(string solicitacao = "Ruído nos freios") =>
        OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), solicitacao, PrioridadeOrdemServico.Normal, null);

    [Fact]
    public void Criar_ComSolicitacaoVazia_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "", PrioridadeOrdemServico.Normal, null));
    }

    [Fact]
    public void Criar_ComSolicitacaoEspacosEmBranco_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "   ", PrioridadeOrdemServico.Normal, null));
    }

    [Fact]
    public void Criar_ComPrazo_DeveArmazenarPrazo()
    {
        var prazo = DateTimeOffset.UtcNow.AddDays(7);

        var os = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), "Serviço", PrioridadeOrdemServico.Alta, prazo);

        Assert.Equal(prazo, os.Prazo);
        Assert.Equal(PrioridadeOrdemServico.Alta, os.Prioridade);
    }

    [Fact]
    public void Cancelar_EmEstadoRecebida_DeveSerPermitido()
    {
        var os = Criar();

        os.AlterarStatus(StatusOrdemServico.Cancelada);

        Assert.Equal(StatusOrdemServico.Cancelada, os.Status);
        Assert.Contains(os.Historico, h => h.Tipo == "OS_CANCELADA");
    }

    [Fact]
    public void Cancelar_EmEstadoEmDiagnostico_DeveSerPermitido()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());

        os.AlterarStatus(StatusOrdemServico.Cancelada);

        Assert.Equal(StatusOrdemServico.Cancelada, os.Status);
    }

    [Fact]
    public void Cancelar_EmEstadoFinalizada_DeveFalhar()
    {
        var os = PassarParaFinalizada();

        var ex = Assert.Throws<RegraNegocioException>(() => os.AlterarStatus(StatusOrdemServico.Cancelada));
        Assert.Equal("OS_NAO_PODE_SER_CANCELADA", ex.Codigo);
    }

    [Fact]
    public void Cancelar_EmEstadoEntregue_DeveFalhar()
    {
        var os = PassarParaFinalizada();
        os.AlterarStatus(StatusOrdemServico.Entregue);

        var ex = Assert.Throws<RegraNegocioException>(() => os.AlterarStatus(StatusOrdemServico.Cancelada));
        Assert.Equal("OS_NAO_PODE_SER_CANCELADA", ex.Codigo);
    }

    [Fact]
    public void IniciarDiagnostico_QuandoNaoRecebida_DeveFalhar()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());

        var ex = Assert.Throws<RegraNegocioException>(() => os.IniciarDiagnostico(Guid.NewGuid()));
        Assert.Equal("STATUS_OS_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void RegistrarDiagnostico_QuandoNaoEmDiagnostico_DeveFalhar()
    {
        var os = Criar();

        var ex = Assert.Throws<RegraNegocioException>(() => os.RegistrarDiagnostico("Diagnóstico"));
        Assert.Equal("STATUS_OS_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void ReceberDecisaoOrcamento_QuandoNaoAguardandoAprovacao_DeveFalhar()
    {
        var os = Criar();

        var ex = Assert.Throws<RegraNegocioException>(() => os.ReceberDecisaoOrcamento(Guid.NewGuid(), true));
        Assert.Equal("STATUS_OS_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void IniciarExecucao_QuandoNaoAguardandoExecucao_DeveFalhar()
    {
        var os = Criar();

        Assert.Throws<RegraNegocioException>(() => os.IniciarExecucao([]));
    }

    [Fact]
    public void AguardarAprovacao_QuandoRecebida_DeveSerPermitido()
    {
        var os = Criar();
        var orcamentoId = Guid.NewGuid();

        os.AguardarAprovacao(orcamentoId, 1);

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        Assert.Contains(os.DomainEvents.OfType<OrcamentoDisponivelDomainEvent>(), e => e.OrcamentoId == orcamentoId);
    }

    [Fact]
    public void AguardarAprovacao_QuandoStatusInvalido_DeveFalhar()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());

        var ex = Assert.Throws<RegraNegocioException>(() => os.AguardarAprovacao(Guid.NewGuid(), 1));
        Assert.Equal("STATUS_OS_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void IniciarExecucao_ComItens_DeveAdicionarDomainEventComItens()
    {
        var os = PassarParaAguardandoExecucao();
        var itens = new List<ItemDeEstoque>
        {
            new(Guid.NewGuid(), 2, "Filtro de óleo")
        };

        os.IniciarExecucao(itens);

        var evento = Assert.Single(os.DomainEvents.OfType<ExecucaoIniciadaDomainEvent>());
        Assert.Single(evento.ItensParaDeduzir);
        Assert.NotNull(os.IniciadaEm);
    }

    [Fact]
    public void AlterarStatus_TransicaoInvalidaParaEntregue_DeveFalhar()
    {
        var os = Criar();

        var ex = Assert.Throws<RegraNegocioException>(() => os.AlterarStatus(StatusOrdemServico.Entregue));
        Assert.Equal("TRANSICAO_STATUS_INVALIDA", ex.Codigo);
    }

    [Fact]
    public void AlterarStatus_ParaAguardandoExecucao_SemPassarPorAprovacao_DeveFalhar()
    {
        var os = Criar();

        var ex = Assert.Throws<RegraNegocioException>(() =>
            os.AlterarStatus(StatusOrdemServico.AguardandoExecucao));
        Assert.Equal("TRANSICAO_STATUS_INVALIDA", ex.Codigo);
    }

    [Fact]
    public void Criar_DeveAdicionarMecanicoIdAoIniciarDiagnostico()
    {
        var os = Criar();
        var mecanicoId = Guid.NewGuid();

        os.IniciarDiagnostico(mecanicoId);

        Assert.Equal(mecanicoId, os.MecanicoId);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
    }

    [Fact]
    public void OrcamentoRejeitado_PermiteNovoOrcamento()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnóstico");

        var primeiroId = Guid.NewGuid();
        os.AguardarAprovacao(primeiroId, 1);
        os.ReceberDecisaoOrcamento(primeiroId, false);

        Assert.Equal(StatusOrdemServico.AguardandoOrcamento, os.Status);
        Assert.Contains(os.DomainEvents.OfType<OrcamentoReprovadoDomainEvent>(), _ => true);
    }

    private static OrdemServico PassarParaFinalizada()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnóstico");
        var orcamentoId = Guid.NewGuid();
        os.AguardarAprovacao(orcamentoId, 1);
        os.ReceberDecisaoOrcamento(orcamentoId, true, estoqueDisponivelParaExecucao: true);
        os.IniciarExecucao([]);
        os.AlterarStatus(StatusOrdemServico.Finalizada);
        return os;
    }

    private static OrdemServico PassarParaAguardandoExecucao()
    {
        var os = Criar();
        os.IniciarDiagnostico(Guid.NewGuid());
        os.RegistrarDiagnostico("Diagnóstico");
        var orcamentoId = Guid.NewGuid();
        os.AguardarAprovacao(orcamentoId, 1);
        os.ReceberDecisaoOrcamento(orcamentoId, true, estoqueDisponivelParaExecucao: true);
        return os;
    }
}
