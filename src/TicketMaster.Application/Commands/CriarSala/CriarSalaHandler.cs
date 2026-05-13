using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Commands.CriarSala;

public sealed class CriarSalaHandler : IRequestHandler<CriarSalaCommand, Guid>
{
    private readonly IRoomRepository _roomRepository;

    public CriarSalaHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Guid> Handle(CriarSalaCommand request, CancellationToken cancellationToken)
    {
        var sala = new Room(request.Nome, request.Layout);
        await _roomRepository.AdicionarAsync(sala, cancellationToken);
        return sala.Id;
    }
}
