using System.Net;
using System.Text.RegularExpressions;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.IntegrationTests.WebApplicationFactory;
using Microsoft.Extensions.DependencyInjection;

namespace TicketMaster.IntegrationTests.ControllerTests;

public sealed class TicketControllerTests
{
    [Fact]
    public async Task Index_QuandoEventoExiste_DeveRetornarPaginaComMapaDeAssentos()
    {
        // Arrange
        await using var factory = new TicketMasterWebFactory($"TicketCtrl_{Guid.NewGuid()}");
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(factory);

        var (eventId, _) = await SeedEventAndTicketsAsync(factory);

        // Act
        var response = await client.GetAsync($"/Ticket/Index?eventId={eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Mapa de Assentos", body);
        Assert.Contains("A1", body);
    }

    [Fact]
    public async Task Index_QuandoEventoNaoExiste_DeveRetornar404()
    {
        // Arrange
        await using var factory = new TicketMasterWebFactory($"TicketCtrl_404_{Guid.NewGuid()}");
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(factory);

        // Act
        var response = await client.GetAsync($"/Ticket/Index?eventId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_DeveExibirPaginaComCodigoDoAssento()
    {
        // Arrange
        await using var factory = new TicketMasterWebFactory($"TicketCtrl_Checkout_{Guid.NewGuid()}");
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(factory);

        var (eventId, _) = await SeedEventAndTicketsAsync(factory);

        // Act
        var response = await client.GetAsync($"/Ticket/Checkout?codigo=A1&eventId={eventId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("A1", body);
        Assert.Contains("Finalizar", body);
    }

    /// <summary>
    /// Cria um evento, sala e ingressos de exemplo no banco InMemory.
    /// </summary>
    private static async Task<(Guid EventId, Guid SalaId)> SeedEventAndTicketsAsync(TicketMasterWebFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var layout = new Room.RoomLayout
        {
            MaxColumns = 3,
            MaxRows = 3,
            Seats = new List<Room.SeatCoordinate>
            {
                new() { SeatCode = "A1", CoordX = 1, CoordY = 1 },
                new() { SeatCode = "A2", CoordX = 2, CoordY = 1 },
            }
        };

        var sala = new Room("Sala Teste", layout);
        context.Rooms.Add(sala);
        await context.SaveChangesAsync();

        var evento = new Event("Evento Teste", DateTime.UtcNow.AddDays(7), sala.Id);
        context.Events.Add(evento);
        await context.SaveChangesAsync();

        context.Tickets.AddRange(
            new Ticket(evento.Id, "A1"),
            new Ticket(evento.Id, "A2")
        );
        await context.SaveChangesAsync();

        return (evento.Id, sala.Id);
    }
}
