using MediatR;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Commands.ExcluirSala;

public sealed record ExcluirSalaCommand(Guid Id) : IRequest<Result>;
