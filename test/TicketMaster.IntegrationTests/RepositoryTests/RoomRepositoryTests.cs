using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;

namespace TicketMaster.IntegrationTests.RepositoryTests;

public sealed class RoomRepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ObterTodosAsync_QuandoHaSalas_DeveRetornarTodas()
    {
        using var context = CreateContext(nameof(ObterTodosAsync_QuandoHaSalas_DeveRetornarTodas));
        var repo = new RoomRepository(context);

        var layout = new Room.RoomLayout { MaxColumns = 3, MaxRows = 3 };
        context.Rooms.AddRange(
            new Room("Sala A", layout),
            new Room("Sala B", layout)
        );
        await context.SaveChangesAsync();

        var salas = await repo.ObterTodosAsync();

        Assert.Equal(2, salas.Count());
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoExiste_DeveRetornarSala()
    {
        using var context = CreateContext(nameof(ObterPorIdAsync_QuandoExiste_DeveRetornarSala));
        var repo = new RoomRepository(context);

        var sala = new Room("Sala Teste", new Room.RoomLayout { MaxColumns = 5, MaxRows = 5 });
        context.Rooms.Add(sala);
        await context.SaveChangesAsync();

        var resultado = await repo.ObterPorIdAsync(sala.Id);

        Assert.NotNull(resultado);
        Assert.Equal("Sala Teste", resultado.Name);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        using var context = CreateContext(nameof(ObterPorIdAsync_QuandoNaoExiste_DeveRetornarNull));
        var repo = new RoomRepository(context);

        var resultado = await repo.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AdicionarAsync_DevePersistirSala()
    {
        using var context = CreateContext(nameof(AdicionarAsync_DevePersistirSala));
        var repo = new RoomRepository(context);

        var layout = new Room.RoomLayout { MaxColumns = 4, MaxRows = 4 };
        var sala = new Room("Sala Nova", layout);
        await repo.AdicionarAsync(sala);

        using var checkContext = CreateContext(nameof(AdicionarAsync_DevePersistirSala));
        var salva = await checkContext.Rooms.FirstOrDefaultAsync(r => r.Name == "Sala Nova");
        Assert.NotNull(salva);
        Assert.Equal(4, salva.Layout.MaxColumns);
    }

    [Fact]
    public async Task AtualizarAsync_DevePersistirAlteracoes()
    {
        using var context = CreateContext(nameof(AtualizarAsync_DevePersistirAlteracoes));
        var repo = new RoomRepository(context);

        var layout = new Room.RoomLayout { MaxColumns = 2, MaxRows = 2 };
        var sala = new Room("Sala Original", layout);
        context.Rooms.Add(sala);
        await context.SaveChangesAsync();

        // Atualiza via reflection (props privadas)
        typeof(Room).GetProperty(nameof(Room.Name))!.SetValue(sala, "Sala Renomeada");
        await repo.AtualizarAsync(sala);

        using var checkContext = CreateContext(nameof(AtualizarAsync_DevePersistirAlteracoes));
        var atualizada = await checkContext.Rooms.FirstAsync(r => r.Id == sala.Id);
        Assert.Equal("Sala Renomeada", atualizada.Name);
    }

    [Fact]
    public async Task RemoverAsync_DeveExcluirSala()
    {
        using var context = CreateContext(nameof(RemoverAsync_DeveExcluirSala));
        var repo = new RoomRepository(context);

        var layout = new Room.RoomLayout { MaxColumns = 2, MaxRows = 2 };
        var sala = new Room("Sala Excluir", layout);
        context.Rooms.Add(sala);
        await context.SaveChangesAsync();

        await repo.RemoverAsync(sala);

        using var checkContext = CreateContext(nameof(RemoverAsync_DeveExcluirSala));
        var excluida = await checkContext.Rooms.FirstOrDefaultAsync(r => r.Id == sala.Id);
        Assert.Null(excluida);
    }

    [Fact]
    public async Task PossuiEventosVinculadosAsync_QuandoTemEvento_DeveRetornarTrue()
    {
        using var context = CreateContext(nameof(PossuiEventosVinculadosAsync_QuandoTemEvento_DeveRetornarTrue));
        var repo = new RoomRepository(context);

        var layout = new Room.RoomLayout { MaxColumns = 2, MaxRows = 2 };
        var sala = new Room("Sala Com Evento", layout);
        context.Rooms.Add(sala);
        await context.SaveChangesAsync();

        var evento = new Event("Evento na Sala", DateTime.UtcNow.AddDays(7), sala.Id);
        context.Events.Add(evento);
        await context.SaveChangesAsync();

        var possui = await repo.PossuiEventosVinculadosAsync(sala.Id);

        Assert.True(possui);
    }

    [Fact]
    public async Task PossuiEventosVinculadosAsync_QuandoSemEvento_DeveRetornarFalse()
    {
        using var context = CreateContext(nameof(PossuiEventosVinculadosAsync_QuandoSemEvento_DeveRetornarFalse));
        var repo = new RoomRepository(context);

        var layout = new Room.RoomLayout { MaxColumns = 2, MaxRows = 2 };
        var sala = new Room("Sala Sem Evento", layout);
        context.Rooms.Add(sala);
        await context.SaveChangesAsync();

        var possui = await repo.PossuiEventosVinculadosAsync(sala.Id);

        Assert.False(possui);
    }
}
