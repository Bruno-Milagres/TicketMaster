using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.Pedidos;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context) => _context = context;

    public List<Pedido> Pedidos { get; set; } = new();

    public async Task OnGetAsync()
    {
        Pedidos = await _context.Pedidos
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAlterarStatusAsync(Guid id, int novoStatus)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound();

        pedido.AlterarStatus((PedidoStatus)novoStatus);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Status alterado!";
        return RedirectToPage();
    }
}
