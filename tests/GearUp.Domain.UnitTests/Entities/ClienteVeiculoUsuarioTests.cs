using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects;
using GearUp.Domain.ValueObjects.Clientes;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class ClienteVeiculoUsuarioTests
{
    [Fact]
    public void Veiculo_DevePertencerAUmCliente()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = Veiculo.Criar(clienteId, "ABC-1D23", "Ford", "Ka", 2020);

        Assert.Equal(clienteId, veiculo.ClienteId);
        Assert.Equal("ABC1D23", veiculo.Placa);
        Assert.True(veiculo.Ativo);
    }

    [Fact]
    public void Veiculo_DeveSerAtualizadoEExcluido()
    {
        var veiculo = Veiculo.Criar(Guid.NewGuid(), "ABC-1D23", "Ford", "Ka", 2020);
        veiculo.Atualizar("DEF4G56", "Fiat", "Pulse", 2024);
        veiculo.Excluir();

        Assert.Equal("DEF4G56", veiculo.Placa);
        Assert.Equal("Fiat", veiculo.Marca);
        Assert.False(veiculo.Ativo);
    }

    [Theory]
    [InlineData("1234567", "Ford", "Ka", 2020)]
    [InlineData("ABC1D23", "", "Ka", 2020)]
    [InlineData("ABC1D23", "Ford", "Ka", 1800)]
    public void Veiculo_ComDadosInvalidos_DeveFalhar(string placa, string marca, string modelo, int ano)
    {
        Assert.Throws<ArgumentException>(() => Veiculo.Criar(Guid.NewGuid(), placa, marca, modelo, ano));
    }

    [Fact]
    public void UsuarioCliente_DeveExigirVinculo()
    {
        Assert.Throws<ArgumentException>(() => Usuario.Criar("cliente", "hash", PerfilUsuario.Cliente));
        var id = Guid.NewGuid();
        var usuario = Usuario.Criar(" CLIENTE ", "hash", PerfilUsuario.Cliente, id);
        Assert.Equal("cliente", usuario.NomeUsuario);
        Assert.Equal(id, usuario.ClienteId);
        Assert.True(usuario.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalido")]
    public void EmailInvalido_DeveFalhar(string valor) => Assert.Throws<ArgumentException>(() => Email.Criar(valor));

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void TelefoneInvalido_DeveFalhar(string valor) => Assert.Throws<ArgumentException>(() => Telefone.Criar(valor));
}
