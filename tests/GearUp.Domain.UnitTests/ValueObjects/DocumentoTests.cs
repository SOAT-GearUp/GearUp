using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Clientes;

namespace GearUp.Domain.UnitTests.ValueObjects;

public sealed class DocumentoTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725", TipoDocumento.Cpf)]
    [InlineData("04.252.011/0001-10", "04252011000110", TipoDocumento.Cnpj)]
    public void Criar_ComDocumentoValido_DeveNormalizar(
        string valor,
        string numeroEsperado,
        TipoDocumento tipoEsperado)
    {
        var documento = Documento.Criar(valor);

        Assert.Equal(numeroEsperado, documento.Numero);
        Assert.Equal(tipoEsperado, documento.Tipo);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("12.345.678/0001-00")]
    [InlineData("123")]
    public void Criar_ComDocumentoInvalido_DeveFalhar(string valor)
    {
        Assert.Throws<ArgumentException>(() => Documento.Criar(valor));
    }
}
