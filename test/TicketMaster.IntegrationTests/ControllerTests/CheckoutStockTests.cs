using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.IntegrationTests.WebApplicationFactory;

namespace TicketMaster.IntegrationTests.ControllerTests;

public sealed class CheckoutStockTests
{
    [Fact]
    public async Task GET_Index_RetornaOK()
    {
        await using var f = new TicketMasterWebFactory($"G_{Guid.NewGuid()}");
        var c = await AuthenticationHelper.CreateAuthenticatedClientAsync(f);
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Checkout/Index")).StatusCode);
    }

    [Fact]
    public async Task POST_EstoqueSuficiente_CriaPedido()
    {
        await using var f = new TicketMasterWebFactory($"OK_{Guid.NewGuid()}");
        var c = await AuthenticationHelper.CreateAuthenticatedClientAsync(f);
        var (eid, tid) = await SeedAsync(f, 10);

        var json = JsonSerializer.Serialize(new[] { new {
            TipoIngressoId = tid.ToString(), EventId = eid.ToString(),
            NomeEvento = "T", NomeIngresso = "Inteira", Preco = 100m, Quantidade = 2
        } });

        var r = await c.PostAsync("/Checkout/Pedido", new FormUrlEncodedContent(
            new Dictionary<string, string> {
                { "clienteNome", "Joao" }, { "clienteEmail", "j@e.com" },
                { "itensJson", json }, { "__RequestVerificationToken", await TokenAsync(c) }
            }));

        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.Contains("/Checkout/Sucesso", r.Headers.Location?.ToString());

        using var s = f.Services.CreateScope();
        var db = s.ServiceProvider.GetRequiredService<AppDbContext>();
        var tipo = await db.TiposIngresso.FindAsync(Guid.Parse(tid.ToString()));
        Assert.Equal(8, tipo!.QuantidadeDisponivel);
    }

    [Fact]
    public async Task POST_EstoqueInsuficiente_ExibeErro()
    {
        await using var f = new TicketMasterWebFactory($"Fail_{Guid.NewGuid()}");
        var c = await AuthenticationHelper.CreateAuthenticatedClientAsync(f);
        var (eid, tid) = await SeedAsync(f, 1);

        var json = JsonSerializer.Serialize(new[] { new {
            TipoIngressoId = tid.ToString(), EventId = eid.ToString(),
            NomeEvento = "T", NomeIngresso = "Inteira", Preco = 100m, Quantidade = 5
        } });

        var body = await (await c.PostAsync("/Checkout/Pedido", new FormUrlEncodedContent(
            new Dictionary<string, string> {
                { "clienteNome", "Joao" }, { "clienteEmail", "j@e.com" },
                { "itensJson", json }, { "__RequestVerificationToken", await TokenAsync(c) }
            }))).Content.ReadAsStringAsync();

        Assert.Contains("estoque", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task POST_IngressoInexistente_ExibeErro()
    {
        await using var f = new TicketMasterWebFactory($"NF_{Guid.NewGuid()}");
        var c = await AuthenticationHelper.CreateAuthenticatedClientAsync(f);

        var json = JsonSerializer.Serialize(new[] { new {
            TipoIngressoId = Guid.NewGuid().ToString(), EventId = Guid.NewGuid().ToString(),
            NomeEvento = "T", NomeIngresso = "X", Preco = 100m, Quantidade = 1
        } });

        var body = await (await c.PostAsync("/Checkout/Pedido", new FormUrlEncodedContent(
            new Dictionary<string, string> {
                { "clienteNome", "Joao" }, { "clienteEmail", "j@e.com" },
                { "itensJson", json }, { "__RequestVerificationToken", await TokenAsync(c) }
            }))).Content.ReadAsStringAsync();

        Assert.Contains("estoque", body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> TokenAsync(HttpClient c)
    {
        var g = await c.GetAsync("/Checkout/Index");
        var m = Regex.Match(await g.Content.ReadAsStringAsync(),
            @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : "";
    }

    private static async Task<(Guid, Guid)> SeedAsync(TicketMasterWebFactory f, int q)
    {
        using var s = f.Services.CreateScope();
        var db = s.ServiceProvider.GetRequiredService<AppDbContext>();
        var ev = new Event("Evento Teste", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        var ti = new TipoIngresso(ev.Id, "Inteira", 100m, q);
        db.TiposIngresso.Add(ti);
        await db.SaveChangesAsync();
        return (ev.Id, ti.Id);
    }
}
