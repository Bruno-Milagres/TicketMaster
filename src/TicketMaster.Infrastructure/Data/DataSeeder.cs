using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Data;

public static class DataSeeder
{
    /// <summary>
    /// Cria roles e administradores no startup (sempre executa).
    /// </summary>
    public static async Task SeedAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        // Cria as roles se não existirem
        foreach (var role in new[] { "AdminGeral", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin Geral — acesso total
        await EnsureUserAsync(userManager, "admin@ticketmaster.com", "admin", "AdminGeral");
        // Admin comum — acesso limitado
        await EnsureUserAsync(userManager, "admin2@ticketmaster.com", "admin", "Admin");
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            // Garante que está na role correta
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
            return;
        }

        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

    /// <summary>
    /// Popula dados de demonstração (salas, eventos, ingressos).
    /// Executa apenas se o banco estiver vazio.
    /// </summary>
    public static async Task SeedDemoDataAsync(AppDbContext context)
    {
        if (await context.Events.AnyAsync())
            return;

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

        var sala = new Room("Cine Master - Sala 01", layout);
        context.Rooms.Add(sala);

        var show = new Event("O Retorno do Tech Lead", DateTime.UtcNow.AddDays(7), sala.Id);
        show.Publicar();
        context.Events.Add(show);

        context.Tickets.AddRange(
            new Ticket(show.Id, "A1"),
            new Ticket(show.Id, "A3"),
            new Ticket(show.Id, "B1"),
            new Ticket(show.Id, "B3")
        );

        await context.SaveChangesAsync();
    }
}
