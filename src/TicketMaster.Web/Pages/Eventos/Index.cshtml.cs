using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Eventos;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Event> Eventos { get; set; } = new();
    public Dictionary<Guid, List<TipoIngresso>> TiposPorEvento { get; set; } = new();

    public async Task OnGetAsync()
    {
        Eventos = await _context.Events
            .Where(e => e.Status == Domain.Enums.EventStatus.Publicado)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        var eventosIds = Eventos.Select(e => e.Id).ToList();
        var tipos = await _context.TiposIngresso
            .Where(t => eventosIds.Contains(t.EventId))
            .ToListAsync();

        TiposPorEvento = tipos
            .GroupBy(t => t.EventId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
