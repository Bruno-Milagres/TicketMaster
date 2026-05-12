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
public class EditarModel : PageModel
{
    private readonly AppDbContext _context;

    public EditarModel(AppDbContext context) => _context = context;

    [BindProperty] public string Titulo { get; set; } = "";
    [BindProperty] public DateTime Data { get; set; }
    [BindProperty] public Guid SalaId { get; set; }
    [BindProperty] public EventStatus Status { get; set; }

    public List<SelectListItem> SalasOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var evento = await _context.Events.FindAsync(id);
        if (evento == null) return NotFound();

        Titulo = evento.Title;
        Data = evento.EventDate;
        SalaId = evento.RoomId;
        Status = evento.Status;

        await CarregarSalas();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid) { await CarregarSalas(); return Page(); }

        var evento = await _context.Events.FindAsync(id);
        if (evento == null) return NotFound();

        // Reflection para atualizar props privadas
        typeof(Event).GetProperty(nameof(Event.Title))!.SetValue(evento, Titulo);
        typeof(Event).GetProperty(nameof(Event.EventDate))!.SetValue(evento, Data.ToUniversalTime());
        typeof(Event).GetProperty(nameof(Event.RoomId))!.SetValue(evento, SalaId);
        typeof(Event).GetProperty(nameof(Event.Status))!.SetValue(evento, Status);

        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Evento atualizado!";
        return RedirectToPage("Index");
    }

    private async Task CarregarSalas()
    {
        var salas = await _context.Rooms.ToListAsync();
        SalasOptions = salas.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
    }
}
