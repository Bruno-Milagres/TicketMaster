using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Services;

public class EventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    //=======================================================================================================
    // Retorna a lista de eventos ativos ordenados por data
    //=======================================================================================================
    public async Task<IEnumerable<Event>> ListarEventosAtivosAsync()
    {
        return await _eventRepository.ListarEventosAtivosAsync();
    }
}