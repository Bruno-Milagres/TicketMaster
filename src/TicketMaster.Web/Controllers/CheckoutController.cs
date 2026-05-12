using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly AppDbContext _db;
    public CheckoutController(AppDbContext db) => _db = db;

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pedido(string clienteNome, string clienteEmail, string itensJson)
    {
        List<CarrinhoItem>? itens;
        try { itens = JsonSerializer.Deserialize<List<CarrinhoItem>>(itensJson); }
        catch { itens = null; }
        if (itens == null || itens.Count == 0)
            return Content("sem itens");

        var pedido = new Pedido(null, clienteNome, clienteEmail);
        foreach (var i in itens)
        {
            var tipo = await _db.TiposIngresso.FirstOrDefaultAsync(t => t.Id == Guid.Parse(i.TipoIngressoId));
            if (tipo == null || tipo.QuantidadeDisponivel < i.Quantidade)
                return Content("sem estoque");
            tipo.ReservarEstoque(i.Quantidade);
            pedido.AdicionarItem(tipo.Id, tipo.Nome, i.Quantidade, tipo.Preco);
        }
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();
        return RedirectToAction("Sucesso");
    }

    public IActionResult Sucesso() => View();

    public class CarrinhoItem
    {
        public string TipoIngressoId { get; set; } = "";
        public string EventId { get; set; } = "";
        public string NomeEvento { get; set; } = "";
        public string NomeIngresso { get; set; } = "";
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}
