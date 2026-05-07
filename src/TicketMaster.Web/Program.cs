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

    // Garante que o banco de dados foi criado
    await context.Database.EnsureCreatedAsync();

    // Insere tickets iniciais caso o banco esteja vazio
    if (!await context.Tickets.AnyAsync())
    {
        context.Tickets.AddRange(
            new Ticket("A1"),
            new Ticket("A2"),
            new Ticket("A3"));

        await context.SaveChangesAsync();
    }
}

//==============================================
// EXECUÇÃO DA APLICAÇÃO
//==============================================
await app.RunAsync();

