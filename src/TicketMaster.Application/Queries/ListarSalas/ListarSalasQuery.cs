using MediatR;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ListarSalas;

public sealed record ListarSalasQuery : IRequest<IEnumerable<Room>>;
