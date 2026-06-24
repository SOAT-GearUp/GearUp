using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class ClienteAdicionaisTests
{
    [Fact]
    public void Atualizar_ComDadosValidos_DeveAlterarNomeEmailTelefone()
    {
        var cliente = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");

        cliente.Atualizar("  João da Silva  ", "joao@email.com", "11988888888");

        Assert.Equal("João da Silva", cliente.Nome);
        Assert.Equal("joao@email.com", cliente.Email.Endereco);
        Assert.Equal("11988888888", cliente.Telefone.Numero);
    }

    [Fact]
    public void Excluir_ClienteJaInativo_DeveSerIdempotente()
    {
        var cliente = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");
        cliente.Excluir();
        var excluidoEmOriginal = cliente.ExcluidoEm;

        cliente.Excluir();

        Assert.Equal(excluidoEmOriginal, cliente.ExcluidoEm);
        Assert.False(cliente.Ativo);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("A")]
    [InlineData("")]
    public void Criar_ComNomeMuitoCurto_DeveFalhar(string nome)
    {
        Assert.Throws<ArgumentException>(() =>
            Cliente.Criar(nome, "52998224725", "maria@email.com", "11999999999"));
    }

    [Fact]
    public void Criar_ComNomeMuitoLongo_DeveFalhar()
    {
        var nomeLongo = new string('A', 151);

        Assert.Throws<ArgumentException>(() =>
            Cliente.Criar(nomeLongo, "52998224725", "maria@email.com", "11999999999"));
    }

    [Fact]
    public void Criar_ComNomeExatamente150Caracteres_DevePermitir()
    {
        var nome = new string('A', 150);

        var cliente = Cliente.Criar(nome, "52998224725", "maria@email.com", "11999999999");

        Assert.Equal(nome, cliente.Nome);
    }

    [Fact]
    public void Criar_ComNomeComEspacos_DeveNormalizar()
    {
        var cliente = Cliente.Criar("  Ana Maria  ", "52998224725", "ana@email.com", "11999999999");

        Assert.Equal("Ana Maria", cliente.Nome);
    }
}
