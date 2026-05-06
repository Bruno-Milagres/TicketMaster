using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;
using TicketMaster.Web.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuração do SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=ticketmaster.db"));

// Dependency Injection
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddHostedService<TicketReaperWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ==========================================
// CRIACAO E SEED DO BANCO
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    // Cria o arquivo ticketmaster.db com a tabela se ele nao existir
    await context.Database.EnsureCreatedAsync();

    // Se o banco estiver vazio, criamos 3 ingressos para brincar
    if (!await context.Tickets.AnyAsync())
    {
        context.Tickets.Add(new Ticket("A1"));
        context.Tickets.Add(new Ticket("A2"));
        context.Tickets.Add(new Ticket("A3"));

        await context.SaveChangesAsync();
        Console.WriteLine("Banco criado e ingressos A1, A2 e A3 gerados com sucesso!");
    }
}

await app.RunAsync();
