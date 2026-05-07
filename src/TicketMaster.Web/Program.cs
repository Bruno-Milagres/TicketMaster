//==============================================
// IMPORTS
//==============================================
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;
using TicketMaster.Web.Consumers;
using TicketMaster.Web.Hubs;
using TicketMaster.Web.Workers;
using Event = TicketMaster.Domain.Entities.Event;

//==============================================
// BUILDER
//==============================================
var builder = WebApplication.CreateBuilder(args);

//==============================================
// SERILOG + OPENTELEMETRY
//==============================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TicketMaster.Web"))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter();
    });

//==============================================
// SERVIÇOS MVC
//==============================================
builder.Services.AddControllersWithViews();

//==============================================
// SIGNALR
//==============================================
builder.Services.AddSignalR();

//==============================================
// IDENTITY
//==============================================
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

//==============================================
// BANCO DE DADOS
//==============================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==============================================
// MENSAGERIA (RABBITMQ + MASSTRANSIT)
// ==============================================
builder.Services.AddMassTransit(x =>
{
    // Registra o nosso robo consumidor
    x.AddConsumer<PagamentoCommandConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Cria as filas no RabbitMQ automaticamente baseada nos nomes dos Consumers
        cfg.ConfigureEndpoints(context);
    });
});

//==============================================
// INJEÇÃO DE DEPENDÊNCIA
//==============================================
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddHostedService<TicketReaperWorker>();

//==============================================
// BUILD
//==============================================
var app = builder.Build();

//==============================================
// TRATAMENTO DE ERROS E SEGURANÇA
//==============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//==============================================
// MIDDLEWARES
//==============================================
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();

//==============================================
// ROTAS
//==============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<TicketHub>("/ticketHub");

//==============================================
// SEED DO BANCO DE DADOS
//==============================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();

    if (!await context.Events.AnyAsync())
    {
        // 1. Criamos o Layout da Sala (3x3 com um corredor no meio)
        var layout = new Room.RoomLayout
        {
            MaxColumns = 3,
            MaxRows = 3,
            Seats = new List<Room.SeatCoordinate>
            {
                new() { SeatCode = "A1", CoordX = 1, CoordY = 1 },
                new() { SeatCode = "A3", CoordX = 3, CoordY = 1 }, // Corredor no X=2
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

//==============================================
// EXECUÇÃO DA APLICAÇÃO
//==============================================
await app.RunAsync();

