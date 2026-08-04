using GearUp.Domain.ValueObjects;
using GearUp.Domain.ValueObjects.Clientes;

namespace GearUp.Domain.UnitTests.ValueObjects;

public sealed class EmailTelefoneTests
{
    [Theory]
    [InlineData("test@example.com", "test@example.com")]
    [InlineData("  USER@DOMAIN.COM  ", "user@domain.com")]
    [InlineData("a@b.co", "a@b.co")]
    [InlineData("nome.sobrenome@empresa.com.br", "nome.sobrenome@empresa.com.br")]
    public void Email_Criar_ComEmailValido_DeveNormalizar(string entrada, string esperado)
    {
        var email = Email.Criar(entrada);

        Assert.Equal(esperado, email.Endereco);
    }

    [Fact]
    public void Email_Criar_ComEmailNulo_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() => Email.Criar(null!));
    }

    [Theory]
    [InlineData("11999999999")]
    [InlineData("1199999999")]
    public void Telefone_Criar_ComTelefoneValido_DeveNormalizar(string entrada)
    {
        var telefone = Telefone.Criar(entrada);

        Assert.Equal(entrada, telefone.Numero);
        Assert.True(telefone.Numero.Length is 10 or 11);
    }

    [Theory]
    [InlineData("(11) 9 9999-9999", "11999999999")]
    [InlineData("(11) 9999-9999", "1199999999")]
    [InlineData("11 9 9999-9999", "11999999999")]
    public void Telefone_Criar_ComTelefoneFormatado_DeveNormalizar(string entrada, string esperado)
    {
        var telefone = Telefone.Criar(entrada);

        Assert.Equal(esperado, telefone.Numero);
    }

    [Fact]
    public void Telefone_Criar_ComTelefoneNulo_DeveFalhar()
    {
        Assert.Throws<ArgumentException>(() => Telefone.Criar(null!));
    }

    [Theory]
    [InlineData("123456789")]    // 9 digits
    [InlineData("123456789012")] // 12 digits
    public void Telefone_Criar_ComQuantidadeDigitosInvalida_DeveFalhar(string entrada)
    {
        Assert.Throws<ArgumentException>(() => Telefone.Criar(entrada));
    }
}
