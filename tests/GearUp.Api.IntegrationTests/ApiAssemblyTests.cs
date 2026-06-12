namespace GearUp.Api.IntegrationTests;

public sealed class ApiAssemblyTests
{
    [Fact]
    public void ApiAssembly_DeveEstarDisponivel()
    {
        var assembly = typeof(Program).Assembly;

        Assert.Equal("GearUp.Api", assembly.GetName().Name);
    }
}
