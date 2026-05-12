using MediatR;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ObterSalaPorId;

public sealed record ObterSalaPorIdQuery(Guid Id) : IRequest<Room?>;
