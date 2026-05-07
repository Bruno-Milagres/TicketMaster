using TicketMaster.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TicketMaster.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Versao é o token de concorrência otimista: o EF Core verifica se mudou antes de salvar.
        modelBuilder.Entity<Ticket>()
            .Property(t => t.Versao)
            .IsConcurrencyToken();
    }
}