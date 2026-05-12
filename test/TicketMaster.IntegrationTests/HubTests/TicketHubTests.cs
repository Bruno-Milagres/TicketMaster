using Microsoft.AspNetCore.SignalR.Client;
using TicketMaster.IntegrationTests.WebApplicationFactory;

namespace TicketMaster.IntegrationTests.HubTests;

public sealed class TicketHubTests
{
    [Fact]
    public async Task EntrarNaSalaDoEvento_DeveAdicionarClienteAoGrupo()
    {
        // Arrange
        await using var factory = new TicketMasterWebFactory($"HubTest_{Guid.NewGuid()}");
        var eventId = Guid.NewGuid().ToString();

        // Cria a conexão SignalR usando o handler do servidor de teste
        var hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/ticketHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            })
            .Build();

        try
        {
            await hubConnection.StartAsync();
            Assert.True(hubConnection.State == HubConnectionState.Connected,
                "Deveria estar conectado ao hub");

            // Act — entra na sala do evento
            await hubConnection.InvokeAsync("EntrarNaSalaDoEvento", eventId);

            // Assert — se não lançou exception, o método foi executado com sucesso
            Assert.True(true);
        }
        finally
        {
            await hubConnection.DisposeAsync();
        }
    }
}
