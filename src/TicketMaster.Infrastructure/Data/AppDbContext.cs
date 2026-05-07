using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Versao é o token de concorrência otimista: o EF Core verifica se mudou antes de salvar.
        builder.Entity<Ticket>()
            .Property(t => t.Versao)
            .IsConcurrencyToken();
    }
}