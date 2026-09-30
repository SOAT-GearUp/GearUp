using GearUp.Domain.Entities;
using GearUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GearUp.Infrastructure.UnitTests.Persistence;

// Verifica o modelo relacional gerado para o PostgreSQL (Fase 3): índices,
// chaves estrangeiras e check constraints que garantem consistência e
// performance. Nenhuma conexão é aberta: apenas o modelo é construído.
public sealed class ModeloRelacionalTests
{
    private static IModel CriarModelo()
    {
        var options = new DbContextOptionsBuilder<GearUpDbContext>()
            .UseNpgsql("Host=localhost;Database=modelo;Username=gearup;Password=gearup")
            .Options;

        using var dbContext = new GearUpDbContext(options);
        return dbContext.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Entidade<T>(IModel modelo) =>
        modelo.FindEntityType(typeof(T)) ?? throw new InvalidOperationException(typeof(T).Name);

    private static IEnumerable<string> Restricoes<T>(IModel modelo) =>
        Entidade<T>(modelo).GetCheckConstraints().Select(restricao => restricao.Name!);

    private static bool PossuiIndice<T>(IModel modelo, params string[] colunas) =>
        Entidade<T>(modelo).GetIndexes().Any(indice =>
            indice.Properties.Select(propriedade => propriedade.Name).SequenceEqual(colunas));

    [Fact]
    public void Clientes_DocumentoDeveSerUnicoERestritoACpfOuCnpj()
    {
        var modelo = CriarModelo();

        var indice = Entidade<Cliente>(modelo).GetIndexes().Single(i => i.GetDatabaseName() == "UX_Clientes_Documento");

        Assert.True(indice.IsUnique);
        Assert.Contains("CK_Clientes_Documento_Tamanho", Restricoes<Cliente>(modelo));
    }

    [Fact]
    public void Estoque_SaldoEPrecoNaoPodemSerNegativos()
    {
        var modelo = CriarModelo();

        var restricoes = Restricoes<Estoque>(modelo).ToList();

        Assert.Contains("CK_EstoqueItens_QuantidadeDisponivel", restricoes);
        Assert.Contains("CK_EstoqueItens_PrecoUnitario", restricoes);
        Assert.Contains("CK_MovimentacoesEstoque_Quantidade", Restricoes<MovimentacaoEstoque>(modelo));
    }

    [Fact]
    public void Orcamentos_ItensEVersaoDevemTerValoresPositivos()
    {
        var modelo = CriarModelo();

        var restricoesItens = Restricoes<ItemOrcamento>(modelo).ToList();

        Assert.Contains("CK_ItensOrcamento_Quantidade", restricoesItens);
        Assert.Contains("CK_ItensOrcamento_ValorUnitario", restricoesItens);
        Assert.Contains("CK_Orcamentos_Versao", Restricoes<Orcamento>(modelo));
    }

    [Fact]
    public void OrdensServico_ListagemPorClienteDeveUsarIndiceComDataDecrescente()
    {
        var modelo = CriarModelo();

        var indice = Entidade<OrdemServico>(modelo).GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual(["ClienteId", "CriadaEm"]));

        Assert.Equal([false, true], indice.IsDescending);
        Assert.False(PossuiIndice<OrdemServico>(modelo, "ClienteId"));
        Assert.True(PossuiIndice<OrdemServico>(modelo, "MecanicoId"));
    }

    [Fact]
    public void Historico_DeveTerIndicePorOrdemServicoEData()
    {
        var modelo = CriarModelo();

        Assert.True(PossuiIndice<HistoricoOrdemServico>(modelo, "OrdemServicoId", "CriadoEm"));
    }

    [Fact]
    public void Notificacoes_DevemReferenciarClienteSemExclusaoEmCascata()
    {
        var modelo = CriarModelo();

        var chave = Entidade<Notificacao>(modelo).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Cliente));

        Assert.Equal(DeleteBehavior.Restrict, chave.DeleteBehavior);
        Assert.True(PossuiIndice<Notificacao>(modelo, "ClienteId", "CriadaEm"));
    }

    [Fact]
    public void ReferenciasEntreContextos_DevemSerIndexadas()
    {
        var modelo = CriarModelo();

        Assert.True(PossuiIndice<MovimentacaoEstoque>(modelo, "OrdemServicoId"));
        Assert.True(PossuiIndice<ItemOrcamento>(modelo, "EstoqueItemId"));
    }

    [Fact]
    public void Migrations_NaoDevemTerAlteracoesPendentesNoModelo()
    {
        var options = new DbContextOptionsBuilder<GearUpDbContext>()
            .UseNpgsql("Host=localhost;Database=modelo;Username=gearup;Password=gearup")
            .Options;

        using var dbContext = new GearUpDbContext(options);

        Assert.False(dbContext.Database.HasPendingModelChanges());
    }
}
