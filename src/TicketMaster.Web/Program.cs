//==============================================
// IMPORTS
//==============================================
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using MediatR;
using TicketMaster.Application;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Cache;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;
using TicketMaster.Web.Consumers;
using TicketMaster.Web.Hubs;
using TicketMaster.Web.Workers;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Distributed;
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
builder.Services.AddRazorPages();

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
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("DefaultConnection") ?? "TestDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

//==============================================
// REDIS CACHE (apenas em não-testes)
//==============================================
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddStackExchangeRedisCache(opts =>
    {
        opts.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        opts.InstanceName = "TicketMaster:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

//==============================================
// COMPRESSÃO + RESPONSE CACHING
//==============================================
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.AddResponseCaching();

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
// Repositórios
builder.Services.AddScoped<TicketRepository>();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<ITicketRepository>(sp =>
        new CachedTicketRepository(
            sp.GetRequiredService<TicketRepository>(),
            sp.GetRequiredService<IDistributedCache>()));
}
else
{
    builder.Services.AddScoped<ITicketRepository>(sp =>
        sp.GetRequiredService<TicketRepository>());
}
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
// Serviços de domínio
builder.Services.AddHostedService<TicketReaperWorker>();
builder.Services.AddScoped<TicketMaster.Application.Interfaces.IQuotaService, TicketMaster.Infrastructure.Services.QuotaService>();

// MediatR + CQRS + FluentValidation
builder.Services.AddApplication();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>());

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
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Login") &&
        context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Redirect("/");
        return;
    }
    await next();
});
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseRouting();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();  // Necessário para Identity (Areas/Identity/Pages)

//==============================================
// ROTAS
//==============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<TicketHub>("/ticketHub");

// ==============================================
// ÁREA ADMIN
// ==============================================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Room}/{action=Index}/{id?}")
    .WithStaticAssets();

//==============================================
// SEED DO BANCO DE DADOS
//==============================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

//==============================================
// EXECUÇÃO DA APLICAÇÃO
//==============================================
await app.RunAsync();

