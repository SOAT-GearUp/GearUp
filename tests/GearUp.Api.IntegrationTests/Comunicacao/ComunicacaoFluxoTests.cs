using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GearUp.Api.IntegrationTests.Infrastructure;
using GearUp.Domain.Enums;

namespace GearUp.Api.IntegrationTests.Comunicacao;

public sealed class ComunicacaoFluxoTests(GearUpApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Notificacoes_DeveListarMensagensParaAtendenteECliente()
    {
        using var adminClient = await CreateAuthenticatedClientAsync();

        var clienteId = await CadastrarClienteAsync(adminClient);
        var veiculoId = await CadastrarVeiculoAsync(adminClient, clienteId);
        var ordemServicoId = await CadastrarOrdemServicoAsync(adminClient, clienteId, veiculoId);

        await CriarUsuarioClienteAsync(adminClient, clienteId, $"cliente.{Guid.NewGuid():N}", "Cliente@123");

        var iniciarDiagnostico = await adminClient.PostAsync($"/api/ordens-servico/{ordemServicoId}/diagnostico/iniciar", null);
        Assert.Equal(HttpStatusCode.NoContent, iniciarDiagnostico.StatusCode);

        var registrarDiagnostico = await adminClient.PostAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/diagnostico", new
        {
            descricao = "Diagnóstico para gerar notificação."
        });
        Assert.Equal(HttpStatusCode.NoContent, registrarDiagnostico.StatusCode);

        var orcamento = await adminClient.PostAsJsonAsync($"/api/ordens-servico/{ordemServicoId}/orcamentos", new
        {
            itens = new[]
            {
                new
                {
                    tipo = TipoItemOrcamento.Servico,
                    descricao = "Serviço comunicado ao cliente",
                    quantidade = 1,
                    valorUnitario = 100m,
                    estoqueItemId = (Guid?)null
                }
            }
        });
        orcamento.EnsureSuccessStatusCode();

        var notificacoesAtendente = await adminClient.GetAsync("/api/notificacoes/notificacoes");
        notificacoesAtendente.EnsureSuccessStatusCode();

        var usuarioCliente = $"cliente.login.{Guid.NewGuid():N}";
        await CriarUsuarioClienteAsync(adminClient, clienteId, usuarioCliente, "Cliente@123");

        using var clienteClient = CreateClient();
        var tokenCliente = await LoginAsync(clienteClient, usuarioCliente, "Cliente@123");
        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenCliente);

        var notificacoesCliente = await clienteClient.GetAsync("/api/notificacoes/notificacoes");
        notificacoesCliente.EnsureSuccessStatusCode();
    }
}
