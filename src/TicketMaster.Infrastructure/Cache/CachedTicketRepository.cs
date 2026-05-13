using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Cache;

public class CachedTicketRepository : ITicketRepository
{
    private readonly ITicketRepository _inner;
    private readonly IDistributedCache _cache;

    public CachedTicketRepository(ITicketRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _inner.ObterPorAssentoAsync(assentoCodigo, eventId, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var key = $"tickets:evento:{eventId}";
        var cached = await _cache.GetStringAsync(key, cancellationToken);
        if (cached != null)
            return JsonSerializer.Deserialize<List<Ticket>>(cached) ?? new List<Ticket>();

        var tickets = await _inner.ObterPorEventoAsync(eventId, cancellationToken);
        var lista = tickets.ToList();

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(lista),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            }, cancellationToken);

        return lista;
    }

    public async Task<IEnumerable<Ticket>> ObterReservasVencidasAsync(CancellationToken cancellationToken = default)
    {
        return await _inner.ObterReservasVencidasAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _inner.AtualizarAsync(ticket, cancellationToken);
        await _cache.RemoveAsync($"tickets:evento:{ticket.EventId}", cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _inner.ObterTodosAsync(cancellationToken);
    }
}
