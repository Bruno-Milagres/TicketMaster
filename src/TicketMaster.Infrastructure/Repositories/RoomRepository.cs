using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rooms.ToListAsync(cancellationToken);
    }

    public async Task<Room?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> PossuiEventosVinculadosAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await _context.Events.AnyAsync(e => e.RoomId == roomId, cancellationToken);
    }
}
