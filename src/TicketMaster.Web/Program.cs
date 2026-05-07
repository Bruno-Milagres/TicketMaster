//==============================================
// IMPORTS
//==============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Infrastructure.Repositories;
using TicketMaster.Web.Workers;

//==============================================
// BUILDER
//==============================================
var builder = WebApplication.CreateBuilder(args);

//==============================================
// SERVIÇOS MVC
//==============================================
builder.Services.AddControllersWithViews();

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

