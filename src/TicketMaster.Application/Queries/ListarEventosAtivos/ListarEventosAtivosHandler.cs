using MediatR;
using TicketMaster.Application.Common;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ListarEventosAtivos;

public sealed class ListarEventosAtivosHandler : IRequestHandler<ListarEventosAtivosQuery, PagedResult<Event>>
{
    private readonly IEventRepository _eventRepository;

    public ListarEventosAtivosHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<PagedResult<Event>> Handle(ListarEventosAtivosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _eventRepository.ListarEventosAtivosAsync(cancellationToken);
        var lista = todos.ToList();
        var total = lista.Count;
        var itens = lista
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .ToList();

        return new PagedResult<Event>(itens, total, request.Pagina, request.TamanhoPagina);
    }
}
