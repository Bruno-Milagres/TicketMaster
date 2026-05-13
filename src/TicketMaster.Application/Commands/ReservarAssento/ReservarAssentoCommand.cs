using MediatR;
using TicketMaster.Domain.Common;
using TicketMaster.Domain.Enums;

namespace TicketMaster.Application.Commands.ReservarAssento;

public sealed record ReservarAssentoCommand(
    string AssentoCodigo,
    Guid UsuarioId,
    Guid EventId,
    TicketCategory Category = TicketCategory.Inteira
) : IRequest<Result>;
