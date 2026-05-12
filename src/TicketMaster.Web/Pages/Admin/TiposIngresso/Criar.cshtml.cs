using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.TiposIngresso;

[Authorize(Roles = "Admin")]
public class CriarModel : PageModel
{
    private readonly AppDbContext _context;

    public CriarModel(AppDbContext context) => _context = context;

    [BindProperty] public Guid EventoId { get; set; }
    [BindProperty] public string Nome { get; set; } = "";
    [BindProperty] public decimal Preco { get; set; }
    [BindProperty] public int Quantidade { get; set; } = 100;

    public List<SelectListItem> EventosOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        await CarregarEventos();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CarregarEventos(); return Page(); }

        var tipo = new TipoIngresso(EventoId, Nome, Preco, Quantidade);
        _context.TiposIngresso.Add(tipo);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Tipo de ingresso criado!";
        return RedirectToPage("Index");
    }

    private async Task CarregarEventos()
    {
        var eventos = await _context.Events.OrderByDescending(e => e.EventDate).ToListAsync();
        EventosOptions = eventos.Select(e => new SelectListItem(e.Title, e.Id.ToString())).ToList();
    }
}
