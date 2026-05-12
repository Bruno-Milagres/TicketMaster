using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;

namespace TicketMaster.IntegrationTests.RepositoryTests;

public sealed class EventRepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ListarEventosAtivos_QuandoHaEventos_DeveRetornarOrdenadosPorData()
    {
        // Arrange
        using var context = CreateContext($"{nameof(EventRepositoryTests)}_Listar_ComEventos");
        var repo = new EventRepository(context);

        var amanha = DateTime.UtcNow.AddDays(1);
        var depois = DateTime.UtcNow.AddDays(7);

        var evento1 = new Event("Evento Futuro", amanha, Guid.NewGuid());
        evento1.Publicar();
        var evento2 = new Event("Evento Depois", depois, Guid.NewGuid());
        evento2.Publicar();
        context.Events.AddRange(evento1, evento2);
        await context.SaveChangesAsync();

        // Act
        var eventos = await repo.ListarEventosAtivosAsync();

        // Assert
        Assert.Equal(2, eventos.Count());
        Assert.Equal("Evento Futuro", eventos.First().Title);
    }

    [Fact]
    public async Task ListarEventosAtivos_QuandoNaoHaEventos_DeveRetornarListaVazia()
    {
        // Arrange
        using var context = CreateContext($"{nameof(EventRepositoryTests)}_Listar_Vazio");
        var repo = new EventRepository(context);

        // Act
        var eventos = await repo.ListarEventosAtivosAsync();

        // Assert
        Assert.Empty(eventos);
    }
}
