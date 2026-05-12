using MediatR;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ListarEventosAtivos;

public sealed record ListarEventosAtivosQuery : IRequest<IEnumerable<Event>>;
