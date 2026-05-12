using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ListarSalas;

public sealed class ListarSalasHandler : IRequestHandler<ListarSalasQuery, IEnumerable<Room>>
{
    private readonly IRoomRepository _roomRepository;

    public ListarSalasHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<Room>> Handle(ListarSalasQuery request, CancellationToken cancellationToken)
    {
        return await _roomRepository.ObterTodosAsync(cancellationToken);
    }
}
