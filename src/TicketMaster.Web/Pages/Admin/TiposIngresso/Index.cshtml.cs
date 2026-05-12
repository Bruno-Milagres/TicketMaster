using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.TiposIngresso;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context) => _context = context;

    public List<TipoIngresso> Tipos { get; set; } = new();

    public async Task OnGetAsync()
    {
        Tipos = await _context.TiposIngresso.Include(t => t.Evento).ToListAsync();
    }

    public async Task<IActionResult> OnPostExcluirAsync(Guid id)
    {
        var tipo = await _context.TiposIngresso.FindAsync(id);
        if (tipo == null) return NotFound();
        _context.TiposIngresso.Remove(tipo);
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Tipo excluído.";
        return RedirectToPage();
    }
}
