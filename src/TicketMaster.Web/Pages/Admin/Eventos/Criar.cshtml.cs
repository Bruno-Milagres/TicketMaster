using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Enums;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Admin.Eventos;

[Authorize(Roles = "Admin")]
public class CriarModel : PageModel
{
    private readonly AppDbContext _context;

    public CriarModel(AppDbContext context) => _context = context;

    [BindProperty] public string Titulo { get; set; } = "";
    [BindProperty] public DateTime Data { get; set; } = DateTime.Now.AddDays(30);
    [BindProperty] public Guid SalaId { get; set; }
    [BindProperty] public EventStatus Status { get; set; } = EventStatus.Rascunho;

    public List<SelectListItem> SalasOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        await CarregarSalas();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CarregarSalas(); return Page(); }

        var evento = new Event(Titulo, Data.ToUniversalTime(), SalaId);
        if (Status == EventStatus.Publicado) evento.Publicar();

        _context.Events.Add(evento);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Evento criado!";
        return RedirectToPage("Index");
    }

    private async Task CarregarSalas()
    {
        var salas = await _context.Rooms.ToListAsync();
        SalasOptions = salas.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
    }
}
