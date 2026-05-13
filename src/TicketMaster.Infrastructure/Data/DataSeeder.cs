using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Enums;

namespace TicketMaster.Infrastructure.Data;

public static class DataSeeder
{
    /// <summary>
    /// Cria roles e administradores no startup (sempre executa).
    /// </summary>
    public static async Task SeedAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        foreach (var role in new[] { "AdminGeral", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, "admin@ticketmaster.com", "admin", "AdminGeral");
        await EnsureUserAsync(userManager, "admin2@ticketmaster.com", "admin", "Admin");
    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, password);
            return;
        }

        user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
        else
            Console.WriteLine($"[SeedAdmin] Erro ao criar {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    /// <summary>
    /// Popula o layout fixo do teatro (1.024 assentos) e o evento de demonstração.
    /// Re-executa se o banco existente estiver com dados legados (sem setores de assento).
    /// </summary>
    public static async Task SeedDemoDataAsync(AppDbContext context)
    {
        // Detecta banco legado: sala existe mas não tem setores definidos
        var salaLegada = await context.Rooms.FirstOrDefaultAsync();
        bool precisaResetar = salaLegada != null &&
                              salaLegada.Layout.Seats.Any() &&
                              salaLegada.Layout.Seats.All(s => string.IsNullOrEmpty(s.Sector));

        if (precisaResetar)
        {
            Console.WriteLine("[Seeder] Banco legado detectado — limpando para re-seed com layout fixo...");
            context.Tickets.RemoveRange(context.Tickets);
            context.EventSectorPrices.RemoveRange(context.EventSectorPrices);
            context.Events.RemoveRange(context.Events);
            context.Rooms.RemoveRange(context.Rooms);
            await context.SaveChangesAsync();
        }
        else if (await context.Events.AnyAsync())
        {
            return; // banco já está correto
        }

        // =====================================================================
        // LAYOUT FIXO DO TEATRO — 1.024 ASSENTOS
        // Setores: PlateiaFrente(120), PlateiaCentro(308), PlateiaFundo(144),
        //          Frisa(48), Camarote(96), Balcao(288), Acessibilidade(20)
        // =====================================================================
        var seats = new List<Room.SeatCoordinate>();
        int gridRow = 1;

        // --- PLATEIA FRENTE: fileiras A–E, 24 colunas (120 assentos) ---
        // Fileiras mais próximas do palco, sem meia-entrada
        var plateiaFrenteLetras = new[] { 'A', 'B', 'C', 'D', 'E' };
        foreach (var letra in plateiaFrenteLetras)
        {
            for (int col = 1; col <= 24; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "Standard",
                    Sector   = Room.Sectors.PlateiaFrente
                });
            gridRow++;
        }

        // --- PLATEIA CENTRO: fileiras F–P, 28 colunas (308 assentos) ---
        var plateiaCentroLetras = new[] { 'F','G','H','I','J','K','L','M','N','O','P' };
        foreach (var letra in plateiaCentroLetras)
        {
            for (int col = 1; col <= 28; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "Standard",
                    Sector   = Room.Sectors.PlateiaCentro
                });
            gridRow++;
        }

        // --- PLATEIA FUNDO: fileiras Q–V, 24 colunas (144 assentos) ---
        var plateiaFundoLetras = new[] { 'Q','R','S','T','U','V' };
        foreach (var letra in plateiaFundoLetras)
        {
            for (int col = 1; col <= 24; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "Standard",
                    Sector   = Room.Sectors.PlateiaFundo
                });
            gridRow++;
        }

        // --- FRISA ESQUERDA: FR-A a FR-D, 6 colunas (24 assentos, VIP) ---
        var frisaLetras = new[] { "A","B","C","D" };
        foreach (var letra in frisaLetras)
        {
            for (int col = 1; col <= 6; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"FRE-{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "VIP",
                    Sector   = Room.Sectors.Frisa
                });
            gridRow++;
        }

        // --- FRISA DIREITA: FR-A a FR-D, 6 colunas (24 assentos, VIP) ---
        foreach (var letra in frisaLetras)
        {
            for (int col = 1; col <= 6; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"FRD-{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "VIP",
                    Sector   = Room.Sectors.Frisa
                });
            gridRow++;
        }

        // --- CAMAROTE ESQUERDO: CAM-A a CAM-F, 8 colunas (48 assentos, VIP) ---
        var camaroteLetras = new[] { "A","B","C","D","E","F" };
        foreach (var letra in camaroteLetras)
        {
            for (int col = 1; col <= 8; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"CAME-{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "VIP",
                    Sector   = Room.Sectors.Camarote
                });
            gridRow++;
        }

        // --- CAMAROTE DIREITO: CAM-A a CAM-F, 8 colunas (48 assentos, VIP) ---
        foreach (var letra in camaroteLetras)
        {
            for (int col = 1; col <= 8; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"CAMD-{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "VIP",
                    Sector   = Room.Sectors.Camarote
                });
            gridRow++;
        }

        // --- BALCÃO: BAL-A a BAL-R, 16 colunas (288 assentos) ---
        var balcaoLetras = new[] {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R"};
        foreach (var letra in balcaoLetras)
        {
            for (int col = 1; col <= 16; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"BAL-{letra}{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "Standard",
                    Sector   = Room.Sectors.Balcao
                });
            gridRow++;
        }

        // --- ACESSIBILIDADE: 4 posições × 5 assentos (20 assentos, Cadeirante) ---
        // Posicionados nas laterais da plateia centro, ao nível do piso
        var accPositions = new[] { "ESQ-F", "ESQ-G", "DIR-F", "DIR-G" };
        foreach (var pos in accPositions)
        {
            for (int col = 1; col <= 5; col++)
                seats.Add(new Room.SeatCoordinate
                {
                    SeatCode = $"AC-{pos}-{col}",
                    CoordX   = col,
                    CoordY   = gridRow,
                    Type     = "Cadeirante",
                    Sector   = Room.Sectors.Acessibilidade
                });
            gridRow++;
        }

        var layout = new Room.RoomLayout
        {
            MaxColumns = 28,
            MaxRows    = gridRow - 1,
            Seats      = seats
        };

        var sala = new Room("Grande Salão — Theatro", layout);
        context.Rooms.Add(sala);

        // =====================================================================
        // EVENTO DE DEMONSTRAÇÃO
        // =====================================================================
        var show = new Event("A Sinfonia do Fim dos Tempos", DateTime.UtcNow.AddDays(30), sala.Id);
        show.Publicar();
        context.Events.Add(show);

        // =====================================================================
        // PREÇOS POR SETOR (Inteira + Meia conforme Lei 12.933/2013)
        // Cota de meia-entrada: 40% da capacidade por setor
        // =====================================================================
        var sectorPrices = new List<EventSectorPrice>
        {
            // Plateia Frente — sem meia (lugares premium próximos ao palco)
            new(show.Id, Room.Sectors.PlateiaFrente,  TicketCategory.Inteira, 180.00m, 120),

            // Plateia Centro — inteira + 40% meia
            new(show.Id, Room.Sectors.PlateiaCentro,  TicketCategory.Inteira,  120.00m, 185),
            new(show.Id, Room.Sectors.PlateiaCentro,  TicketCategory.Meia,      60.00m,  123),

            // Plateia Fundo — inteira + 40% meia
            new(show.Id, Room.Sectors.PlateiaFundo,   TicketCategory.Inteira,   80.00m,  87),
            new(show.Id, Room.Sectors.PlateiaFundo,   TicketCategory.Meia,      40.00m,  57),

            // Frisa (VIP) — sem meia
            new(show.Id, Room.Sectors.Frisa,          TicketCategory.Inteira,  350.00m,  48),

            // Camarote (VIP) — sem meia
            new(show.Id, Room.Sectors.Camarote,       TicketCategory.Inteira,  500.00m,  96),

            // Balcão — inteira + 40% meia
            new(show.Id, Room.Sectors.Balcao,         TicketCategory.Inteira,   60.00m, 173),
            new(show.Id, Room.Sectors.Balcao,         TicketCategory.Meia,      30.00m, 115),

            // Acessibilidade — gratuito/meia conforme legislação
            new(show.Id, Room.Sectors.Acessibilidade, TicketCategory.Meia,       0.00m,  20),
        };
        context.EventSectorPrices.AddRange(sectorPrices);

        // =====================================================================
        // TICKETS — um por assento
        // =====================================================================
        var tickets = seats.Select(s => new Ticket(show.Id, s.SeatCode)).ToList();
        context.Tickets.AddRange(tickets);

        await context.SaveChangesAsync();

        Console.WriteLine($"[Seeder] Teatro gerado: {seats.Count} assentos, {sectorPrices.Count} faixas de preço.");
    }
}

