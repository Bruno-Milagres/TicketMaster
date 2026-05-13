using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<TipoIngresso> TiposIngresso { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PrecoHistorico> PrecosHistoricos { get; set; }

    //=========================================================
    // Cria as tabelas baseadas nas entidades e configurações
    //=========================================================
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>()
            .OwnsOne(r => r.Layout, layout =>
            {
                layout.ToJson(); 
                layout.OwnsMany(l => l.Seats);
            });

        builder.Entity<Ticket>()
            .Property(t => t.Versao)
            .IsConcurrencyToken();
    }
}