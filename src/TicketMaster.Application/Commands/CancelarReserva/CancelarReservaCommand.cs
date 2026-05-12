using MediatR;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Commands.CancelarReserva;

public sealed record CancelarReservaCommand(
    string AssentoCodigo,
    Guid UsuarioId,
    Guid EventId
) : IRequest<Result>;
