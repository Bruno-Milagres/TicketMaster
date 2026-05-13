using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Interfaces;

public interface IQuotaService
{
    Task<Result> VerificarMeiaEntradaAsync(Guid eventId, string seatCode, CancellationToken ct);
    Task IncrementarMeiaEntradaAsync(Guid eventId, string seatCode, CancellationToken ct);
}
