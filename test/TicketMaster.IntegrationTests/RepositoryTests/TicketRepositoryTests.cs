using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Exceptions;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;

namespace TicketMaster.IntegrationTests.RepositoryTests;

public sealed class TicketRepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private readonly Guid _eventId = Guid.NewGuid();

    [Fact]
    public async Task ObterPorAssentoAsync_QuandoExiste_DeveRetornarTicket()
    {
        // Arrange
        using var context = CreateContext($"{nameof(TicketRepositoryTests)}_ObterPorAssento_Existe");
        var repo = new TicketRepository(context);

        var ticket = new Ticket(_eventId, "A1");
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        // Act
        var resultado = await repo.ObterPorAssentoAsync("A1", _eventId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("A1", resultado.AssentoCodigo);
        Assert.Equal(TicketStatus.Disponivel, resultado.Status);
    }

    [Fact]
    public async Task ObterPorAssentoAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        using var context = CreateContext($"{nameof(TicketRepositoryTests)}_ObterPorAssento_Null");
        var repo = new TicketRepository(context);

        // Act
        var resultado = await repo.ObterPorAssentoAsync("Z9", _eventId);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorEventoAsync_DeveRetornarApenasTicketsDoEvento()
    {
        // Arrange
        using var context = CreateContext($"{nameof(TicketRepositoryTests)}_PorEvento");
        var repo = new TicketRepository(context);

        var outroEvento = Guid.NewGuid();
        context.Tickets.AddRange(
            new Ticket(_eventId, "A1"),
            new Ticket(_eventId, "A2"),
            new Ticket(outroEvento, "B1")
        );
        await context.SaveChangesAsync();

        // Act
        var tickets = await repo.ObterPorEventoAsync(_eventId);

        // Assert
        Assert.Equal(2, tickets.Count());
    }

    [Fact]
    public async Task ObterReservasVencidasAsync_DeveRetornarApenasReservasExpiradas()
    {
        // Arrange
        using var context = CreateContext($"{nameof(TicketRepositoryTests)}_ReservasVencidas");
        var repo = new TicketRepository(context);

        var ticketVencido = new Ticket(_eventId, "A1");
        ticketVencido.Reservar(Guid.NewGuid());
        typeof(Ticket)
            .GetProperty(nameof(Ticket.DataExpiraReserva))!
            .SetValue(ticketVencido, DateTime.UtcNow.AddMinutes(-5));

        var ticketValido = new Ticket(_eventId, "A2");
        ticketValido.Reservar(Guid.NewGuid());
        // DataExpiraReserva já é UtcNow + 15 min (válida)

        context.Tickets.AddRange(ticketVencido, ticketValido);
        await context.SaveChangesAsync();

        // Act
        var vencidos = await repo.ObterReservasVencidasAsync();

        // Assert
        var lista = vencidos.ToList();
        Assert.Single(lista);
        Assert.Equal("A1", lista[0].AssentoCodigo);
    }

    [Fact]
    public async Task AtualizarAsync_DevePersistirAlteracoes()
    {
        // Arrange
        using var context = CreateContext($"{nameof(TicketRepositoryTests)}_Atualizar");
        var repo = new TicketRepository(context);

        var ticket = new Ticket(_eventId, "A1");
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        // Act — modifica o ticket e persiste
        ticket.Reservar(Guid.NewGuid());
        await repo.AtualizarAsync(ticket);

        // Assert
        using var checkContext = CreateContext($"{nameof(TicketRepositoryTests)}_Atualizar");
        var atualizado = await checkContext.Tickets.FirstAsync(t => t.AssentoCodigo == "A1");
        Assert.Equal(TicketStatus.Reservado, atualizado.Status);
    }
}
