using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo)
    {
        // Vai no banco e busca o ingresso pelo codigo (ex: "A1")
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.AssentoCodigo == assentoCodigo);
    }

    public async Task AtualizarAsync(Ticket ticket)
    {
        // Manda o EF Core salvar as mudanças. 
        // Se alguem ja tiver mudado a "Versao" no banco antes de voce, ele vai dar o erro de concorrencia aqui!
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Ticket>> ObterTodosAsync()
    {
        return await _context.Tickets.ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> ObterReservasVencidasAsync()
    {
        // Traz todos que estao Reservados E cuja data de expiracao ja ficou no passado
        return await _context.Tickets
            .Where(t => t.Status == TicketStatus.Reservado && t.DataExpiraReserva <= DateTime.UtcNow)
            .ToListAsync();
    }
}