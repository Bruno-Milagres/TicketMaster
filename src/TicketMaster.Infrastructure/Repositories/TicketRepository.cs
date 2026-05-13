using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Exceptions;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    //=======================================================================================================
    // Retorna o ingresso pelo código do assento e ID do evento.
    // Retorna null se não encontrar o ingresso.
    //=======================================================================================================
    public async Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.AssentoCodigo == assentoCodigo && t.EventId == eventId, cancellationToken);
    }

    //=======================================================================================================
    // Retorna o ingresso pelo ID.
    // Retorna null se não encontrar o ingresso.
    //=======================================================================================================
    public async Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    //=======================================================================================================
    // Retorna todos os ingressos.
    //=======================================================================================================
    public async Task<IEnumerable<Ticket>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tickets.ToListAsync(cancellationToken);
    }

    //=======================================================================================================
    // Retorna os ingressos que estão reservados e cuja reserva já expirou.
    //=======================================================================================================
    public async Task<IEnumerable<Ticket>> ObterReservasVencidasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.Status == TicketStatus.Reservado && t.DataExpiraReserva <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    //=======================================================================================================
    // Persiste as alterações do ingresso.
    // Lança <see cref="ConcurrencyException"/> se outro processo alterou o registro simultaneamente.       
    //=======================================================================================================
    public async Task AtualizarAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Conflito de concorrência ao salvar o ingresso.", ex);
        }
    }
}