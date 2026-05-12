using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Enums;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.Dashboard;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context) => _context = context;

    public int PedidosNoMes { get; set; }
    public int IngressosVendidos { get; set; }
    public int EventosAtivos { get; set; }
    public decimal ReceitaTotal { get; set; }
    public List<string> EventosLabels { get; set; } = new();
    public List<int> EventosData { get; set; } = new();

    public async Task OnGetAsync()
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        PedidosNoMes = await _context.Pedidos
            .CountAsync(p => p.DataPedido >= inicioMes);

        IngressosVendidos = await _context.ItensPedido
            .Include(i => i.Pedido)
            .Where(i => i.Pedido.DataPedido >= inicioMes)
            .SumAsync(i => i.Quantidade);

        EventosAtivos = await _context.Events
            .CountAsync(e => e.Status == EventStatus.Publicado);

        ReceitaTotal = await _context.Pedidos
            .Where(p => p.Status == PedidoStatus.Pago || p.Status == PedidoStatus.Enviado)
            .SumAsync(p => p.Total);

        var vendasPorEvento = await _context.ItensPedido
            .GroupBy(i => i.NomeIngresso)
            .Select(g => new { Nome = g.Key, Total = g.Sum(i => i.Quantidade) })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToListAsync();

        EventosLabels = vendasPorEvento.Select(x => x.Nome).ToList();
        EventosData = vendasPorEvento.Select(x => x.Total).ToList();
    }
}
