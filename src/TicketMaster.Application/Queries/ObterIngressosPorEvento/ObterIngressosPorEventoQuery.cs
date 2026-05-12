using MediatR;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ObterIngressosPorEvento;

public sealed record ObterIngressosPorEventoQuery(Guid EventId) : IRequest<IEnumerable<Ticket>>;
