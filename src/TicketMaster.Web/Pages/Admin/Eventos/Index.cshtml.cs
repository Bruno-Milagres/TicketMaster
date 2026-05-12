using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Enums;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.Eventos;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context) => _context = context;

    public List<Event> Eventos { get; set; } = new();

    public async Task OnGetAsync()
    {
        Eventos = await _context.Events.OrderByDescending(e => e.EventDate).ToListAsync();
    }

    public async Task<IActionResult> OnPostExcluirAsync(Guid id)
    {
        var evento = await _context.Events.FindAsync(id);
        if (evento == null) return NotFound();

        _context.Events.Remove(evento);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Evento excluído.";
        return RedirectToPage();
    }
}
