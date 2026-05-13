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
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
// LIMITE UPLOAD
//==============================================
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.MultipartBodyLengthLimit = 5_242_880); // 5MB

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
// SWAGGER/OPENAPI
//==============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new() { Title = "TicketMaster API", Version = "v1" });
    opts.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    opts.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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
// IDENTITY + JWT
//==============================================
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 5;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication()
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ticketmaster",
            ValidateAudience = true,
            ValidAudience = "ticketmaster-api",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "ChaveSuperSecretaTicketMaster2026!"))
        };
    });

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

// Token Service
builder.Services.AddScoped<TicketMaster.Web.Services.TokenService>();
builder.Services.AddScoped<TicketMaster.Web.Services.QrCodeService>();

// Email Service
builder.Services.AddScoped<TicketMaster.Application.Interfaces.IEmailService, TicketMaster.Infrastructure.Services.LogEmailService>();

// MediatR + CQRS + FluentValidation
builder.Services.AddApplication();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.RegisterServicesFromAssembly(typeof(TicketMaster.Application.DependencyInjection).Assembly);
});

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketMaster v1"));
}

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
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        await context.Database.MigrateAsync();
        await DataSeeder.SeedAdminAsync(roleManager, userManager);
        await DataSeeder.SeedDemoDataAsync(context);
    }
}

//==============================================
// EXECUÇÃO DA APLICAÇÃO
//==============================================
await app.RunAsync();

