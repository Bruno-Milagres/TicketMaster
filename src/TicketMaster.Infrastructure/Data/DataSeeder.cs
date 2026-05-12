using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Data;

/// <summary>
/// Responsável por semear o banco de dados com dados iniciais (salas, eventos e ingressos).
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Cria o banco se não existir
        await context.Database.EnsureCreatedAsync();

        if (await context.Events.AnyAsync())
            return; // Já possui dados — não semear novamente

        // 1. Criamos o Layout da Sala (3x3 com um corredor no meio)
        var layout = new Room.RoomLayout
        {
            MaxColumns = 3,
            MaxRows = 3,
            Seats = new List<Room.SeatCoordinate>
            {
                new() { SeatCode = "A1", CoordX = 1, CoordY = 1 },
                new() { SeatCode = "A3", CoordX = 3, CoordY = 1 },
                new() { SeatCode = "B1", CoordX = 1, CoordY = 2 },
                new() { SeatCode = "B3", CoordX = 3, CoordY = 2 }
            }
        };

        // 2. Criamos a Sala
        var sala = new Room("Cine Master - Sala 01", layout);
        context.Rooms.Add(sala);

        // 3. Criamos o Evento vinculado à Sala
        var show = new Event("O Retorno do Tech Lead", DateTime.UtcNow.AddDays(7), sala.Id);
        context.Events.Add(show);

        // 4. Criamos os Ingressos vinculados ao Evento
        context.Tickets.AddRange(
            new Ticket(show.Id, "A1"),
            new Ticket(show.Id, "A3"),
            new Ticket(show.Id, "B1"),
            new Ticket(show.Id, "B3")
        );

        await context.SaveChangesAsync();
    }
}
