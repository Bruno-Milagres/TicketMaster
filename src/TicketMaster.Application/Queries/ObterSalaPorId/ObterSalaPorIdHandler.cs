using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ObterSalaPorId;

public sealed class ObterSalaPorIdHandler : IRequestHandler<ObterSalaPorIdQuery, Room?>
{
    private readonly IRoomRepository _roomRepository;

    public ObterSalaPorIdHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Room?> Handle(ObterSalaPorIdQuery request, CancellationToken cancellationToken)
    {
        return await _roomRepository.ObterPorIdAsync(request.Id, cancellationToken);
    }
}
