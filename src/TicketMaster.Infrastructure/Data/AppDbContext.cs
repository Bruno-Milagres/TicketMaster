using TicketMaster.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TicketMaster.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // R1. O construtor e obrigatório. Ele recebe as opções do Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // R2. Aqui nos dizemos ao EF: "Crie uma tabela chamada Tickets baseada nesta classe"
    public DbSet<Ticket> Tickets { get; set; }

    // R3. Fluent API (Configuracoes avancadas da tabela)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Versao)
            .IsConcurrencyToken();
    }
}