using Microsoft.Extensions.Caching.Distributed;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Cache;

public class CachedTicketRepository : ITicketRepository
{
    private readonly ITicketRepository _inner;

    public CachedTicketRepository(ITicketRepository inner, IDistributedCache cache)
    {
        _inner = inner;
    }

    public async Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _inner.ObterPorAssentoAsync(assentoCodigo, eventId, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        // Cache desabilitado temporariamente — Ticket tem private setters
        // e System.Text.Json não desserializa corretamente. Bug causava
        // mapa com todos os assentos disponíveis ao recarregar.
        return await _inner.ObterPorEventoAsync(eventId, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> ObterReservasVencidasAsync(CancellationToken cancellationToken = default)
    {
        return await _inner.ObterReservasVencidasAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _inner.AtualizarAsync(ticket, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _inner.ObterTodosAsync(cancellationToken);
    }
}
