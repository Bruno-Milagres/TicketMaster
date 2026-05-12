using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.TiposIngresso;

[Authorize(Roles = "Admin")]
public class EditarModel : PageModel
{
    private readonly AppDbContext _context;

    public EditarModel(AppDbContext context) => _context = context;

    [BindProperty] public Guid EventoId { get; set; }
    [BindProperty] public string Nome { get; set; } = "";
    [BindProperty] public decimal Preco { get; set; }
    [BindProperty] public int Quantidade { get; set; }

    public List<SelectListItem> EventosOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var tipo = await _context.TiposIngresso.FindAsync(id);
        if (tipo == null) return NotFound();

        EventoId = tipo.EventId;
        Nome = tipo.Nome;
        Preco = tipo.Preco;
        Quantidade = tipo.QuantidadeDisponivel;
        await CarregarEventos();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid) { await CarregarEventos(); return Page(); }

        var tipo = await _context.TiposIngresso.FindAsync(id);
        if (tipo == null) return NotFound();

        typeof(TipoIngresso).GetProperty(nameof(TipoIngresso.EventId))!.SetValue(tipo, EventoId);
        typeof(TipoIngresso).GetProperty(nameof(TipoIngresso.Nome))!.SetValue(tipo, Nome);
        typeof(TipoIngresso).GetProperty(nameof(TipoIngresso.Preco))!.SetValue(tipo, Preco);
        tipo.Atualizar(Nome, Preco, Quantidade);

        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Tipo atualizado!";
        return RedirectToPage("Index");
    }

    private async Task CarregarEventos()
    {
        var eventos = await _context.Events.OrderByDescending(e => e.EventDate).ToListAsync();
        EventosOptions = eventos.Select(e => new SelectListItem(e.Title, e.Id.ToString())).ToList();
    }
}
