using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

public interface IRoomRepository
{
    Task<IEnumerable<Room>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Room?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Room room, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Room room, CancellationToken cancellationToken = default);
    Task RemoverAsync(Room room, CancellationToken cancellationToken = default);
    Task<bool> PossuiEventosVinculadosAsync(Guid roomId, CancellationToken cancellationToken = default);
}
