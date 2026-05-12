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
    public async Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.AssentoCodigo == assentoCodigo && t.EventId == eventId);
    }

    //=======================================================================================================
    // Retorna o ingresso pelo ID.
    // Retorna null se não encontrar o ingresso.
    //=======================================================================================================
    public async Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId)
    {
        return await _context.Tickets
            .Where(t => t.EventId == eventId)
            .ToListAsync();
    }

    //=======================================================================================================
    // Retorna todos os ingressos.
    //=======================================================================================================
    public async Task<IEnumerable<Ticket>> ObterTodosAsync()
    {
        return await _context.Tickets.ToListAsync();
    }

    //=======================================================================================================
    // Retorna os ingressos que estão reservados e cuja reserva já expirou.
    //=======================================================================================================
    public async Task<IEnumerable<Ticket>> ObterReservasVencidasAsync()
    {
        return await _context.Tickets
            .Where(t => t.Status == TicketStatus.Reservado && t.DataExpiraReserva <= DateTime.UtcNow)
            .ToListAsync();
    }

    //=======================================================================================================
    // Persiste as alterações do ingresso.
    // Lança <see cref="ConcurrencyException"/> se outro processo alterou o registro simultaneamente.       
    //=======================================================================================================
    public async Task AtualizarAsync(Ticket ticket)
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Conflito de concorrência ao salvar o ingresso.", ex);
        }
    }
}