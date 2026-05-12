using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ListarEventosAtivos;

public sealed class ListarEventosAtivosHandler : IRequestHandler<ListarEventosAtivosQuery, IEnumerable<Event>>
{
    private readonly IEventRepository _eventRepository;

    public ListarEventosAtivosHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<Event>> Handle(ListarEventosAtivosQuery request, CancellationToken cancellationToken)
    {
        return await _eventRepository.ListarEventosAtivosAsync(cancellationToken);
    }
}
