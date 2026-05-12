using MediatR;
using TicketMaster.Application.Interfaces;

namespace TicketMaster.Application.Commands.AtualizarSala;

public sealed class AtualizarSalaHandler : IRequestHandler<AtualizarSalaCommand>
{
    private readonly IRoomRepository _roomRepository;

    public AtualizarSalaHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task Handle(AtualizarSalaCommand request, CancellationToken cancellationToken)
    {
        var sala = await _roomRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (sala == null)
            throw new KeyNotFoundException($"Sala {request.Id} não encontrada.");

        // Reflection para atualizar propriedades privates
        typeof(Domain.Entities.Room)
            .GetProperty(nameof(Domain.Entities.Room.Name))!
            .SetValue(sala, request.Nome);

        typeof(Domain.Entities.Room)
            .GetProperty(nameof(Domain.Entities.Room.Layout))!
            .SetValue(sala, request.Layout);

        await _roomRepository.AtualizarAsync(sala, cancellationToken);
    }
}
