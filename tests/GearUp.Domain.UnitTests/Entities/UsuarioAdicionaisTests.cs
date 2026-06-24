using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Domain.UnitTests.Entities;

public sealed class UsuarioAdicionaisTests
{
    [Theory]
    [InlineData(PerfilUsuario.Atendente)]
    [InlineData(PerfilUsuario.Auxiliar)]
    [InlineData(PerfilUsuario.Mecanico)]
    public void Criar_ComPerfilNaoCliente_NaoExigeClienteId(PerfilUsuario perfil)
    {
        var usuario = Usuario.Criar("funcionario01", "hash123", perfil);

        Assert.Equal("funcionario01", usuario.NomeUsuario);
        Assert.Null(usuario.ClienteId);
        Assert.Equal(perfil, usuario.Perfil);
        Assert.True(usuario.Ativo);
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Fact]
    public void Criar_ComNomeComEspacos_DeveNormalizar()
    {
        var usuario = Usuario.Criar("  MECANICO_01  ", "hash", PerfilUsuario.Mecanico);

        Assert.Equal("mecanico_01", usuario.NomeUsuario);
    }

    [Fact]
    public void Criar_ComSenhaVazia_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar("usuario", "", PerfilUsuario.Atendente));
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar("", "hash", PerfilUsuario.Atendente));
    }

    [Fact]
    public void Criar_ComNomeEspacos_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar("   ", "hash", PerfilUsuario.Atendente));
    }

    [Fact]
    public void Criar_ClienteComClienteId_DeveCriarComVinculo()
    {
        var clienteId = Guid.NewGuid();

        var usuario = Usuario.Criar("cliente01", "hash", PerfilUsuario.Cliente, clienteId);

        Assert.Equal(clienteId, usuario.ClienteId);
        Assert.Equal(PerfilUsuario.Cliente, usuario.Perfil);
    }
}
