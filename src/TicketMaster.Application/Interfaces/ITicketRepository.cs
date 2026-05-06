using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo);
    Task<IEnumerable<Ticket>> ObterTodosAsync();
    Task<IEnumerable<Ticket>> ObterReservasVencidasAsync();

    /// <summary>Persiste as alterações de um ingresso. Lança <see cref="Domain.Exceptions.ConcurrencyException"/> em conflito.</summary>
    Task AtualizarAsync(Ticket ticket);
}

