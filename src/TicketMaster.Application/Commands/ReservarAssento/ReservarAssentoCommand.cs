using MediatR;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Commands.ReservarAssento;

public sealed record ReservarAssentoCommand(
    string AssentoCodigo,
    Guid UsuarioId,
    Guid EventId
) : IRequest<Result>;
