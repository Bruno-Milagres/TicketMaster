using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ObterIngressosPorEvento;

public sealed class ObterIngressosPorEventoHandler : IRequestHandler<ObterIngressosPorEventoQuery, IEnumerable<Ticket>>
{
    private readonly ITicketRepository _ticketRepository;

    public ObterIngressosPorEventoHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<Ticket>> Handle(ObterIngressosPorEventoQuery request, CancellationToken cancellationToken)
    {
        return await _ticketRepository.ObterPorEventoAsync(request.EventId, cancellationToken);
    }
}
