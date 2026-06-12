using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class ClienteTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarClienteAtivo()
    {
        var cliente = Cliente.Criar(
            "  Maria da Silva  ",
            "529.982.247-25",
            "MARIA@EMAIL.COM",
            "(11) 99999-9999");

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("Maria da Silva", cliente.Nome);
        Assert.Equal("52998224725", cliente.Documento.Numero);
        Assert.Equal(TipoDocumento.Cpf, cliente.Documento.Tipo);
        Assert.Equal("maria@email.com", cliente.Email.Endereco);
        Assert.Equal("11999999999", cliente.Telefone.Numero);
        Assert.True(cliente.Ativo);
        Assert.Null(cliente.ExcluidoEm);
    }

    [Fact]
    public void Excluir_DeveRealizarExclusaoLogica()
    {
        var cliente = Cliente.Criar(
            "Maria da Silva",
            "52998224725",
            "maria@email.com",
            "11999999999");

        cliente.Excluir();

        Assert.False(cliente.Ativo);
        Assert.NotNull(cliente.ExcluidoEm);
    }
}
