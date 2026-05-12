using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Commands.ExcluirSala;

public sealed class ExcluirSalaHandler : IRequestHandler<ExcluirSalaCommand, Result>
{
    private readonly IRoomRepository _roomRepository;

    public ExcluirSalaHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result> Handle(ExcluirSalaCommand request, CancellationToken cancellationToken)
    {
        var sala = await _roomRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (sala == null)
            return Result.Failure("Sala não encontrada.");

        var possuiEventos = await _roomRepository.PossuiEventosVinculadosAsync(request.Id, cancellationToken);
        if (possuiEventos)
            return Result.Failure("Não é possível excluir uma sala que possui eventos vinculados.");

        await _roomRepository.RemoverAsync(sala, cancellationToken);
        return Result.Success();
    }
}
