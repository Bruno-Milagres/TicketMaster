using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    // Retorna os eventos ordenados por data
    public async Task<IEnumerable<Event>> ListarEventosAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }
}