using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Notifications;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Commands.CancelarReserva;

public sealed class CancelarReservaHandler : IRequestHandler<CancelarReservaCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPublisher _publisher;

    public CancelarReservaHandler(ITicketRepository ticketRepository, IPublisher publisher)
    {
        _ticketRepository = ticketRepository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(CancelarReservaCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(
            request.AssentoCodigo, request.EventId, cancellationToken);

        if (ticket == null)
            return Result.Failure("Assento não encontrado.");

        var resultado = ticket.CancelarReservaPeloUsuario(request.UsuarioId);

        if (!resultado.IsSuccess)
            return resultado;

        await _ticketRepository.AtualizarAsync(ticket, cancellationToken);

        await _publisher.Publish(new ReservaCanceladaNotification(
            request.EventId, request.AssentoCodigo, "Disponivel"), cancellationToken);

        return Result.Success();
    }
}
