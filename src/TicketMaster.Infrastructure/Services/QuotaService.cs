using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Common;
using TicketMaster.Domain.Enums;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Infrastructure.Services;

public class QuotaService : IQuotaService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;

    public QuotaService(AppDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Result> VerificarMeiaEntradaAsync(Guid eventId, string seatCode, CancellationToken ct)
    {
        var sector = GetSectorFromSeatCode(seatCode);
        if (sector == null) return Result.Success();

        var maxQuota = await _db.EventSectorPrices
            .Where(p => p.EventId == eventId && p.Sector == sector && p.Category == TicketCategory.Meia)
            .Select(p => p.MaxQuota)
            .FirstOrDefaultAsync(ct);

        if (maxQuota <= 0) return Result.Success();

        var quotaKey = $"quota:meia:{eventId}:{sector}";
        var currentStr = await _cache.GetStringAsync(quotaKey, ct);
        var current = currentStr != null ? int.Parse(currentStr) : 0;

        return current >= maxQuota
            ? Result.Failure("Cota de meia-entrada esgotada para este setor.")
            : Result.Success();
    }

    public async Task IncrementarMeiaEntradaAsync(Guid eventId, string seatCode, CancellationToken ct)
    {
        var sector = GetSectorFromSeatCode(seatCode);
        if (sector == null) return;

        var quotaKey = $"quota:meia:{eventId}:{sector}";
        var currentStr = await _cache.GetStringAsync(quotaKey, ct);
        var current = currentStr != null ? int.Parse(currentStr) : 0;
        await _cache.SetStringAsync(quotaKey, (current + 1).ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        }, ct);
    }

    private static string? GetSectorFromSeatCode(string seatCode)
    {
        var parts = seatCode.Split('-');
        return parts.Length >= 3 ? parts[1] : null;
    }
}
