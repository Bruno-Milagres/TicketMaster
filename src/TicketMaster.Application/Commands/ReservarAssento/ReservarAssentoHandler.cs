using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Notifications;
using TicketMaster.Domain.Common;
using TicketMaster.Domain.Exceptions;

namespace TicketMaster.Application.Commands.ReservarAssento;

public sealed class ReservarAssentoHandler : IRequestHandler<ReservarAssentoCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPublisher _publisher;

    public ReservarAssentoHandler(ITicketRepository ticketRepository, IPublisher publisher)
    {
        _ticketRepository = ticketRepository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(ReservarAssentoCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(
            request.AssentoCodigo, request.EventId, cancellationToken);

        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");

        var resultado = ticket.Reservar(request.UsuarioId);

        if (!resultado.IsSuccess)
            return resultado;

        try
        {
            await _ticketRepository.AtualizarAsync(ticket, cancellationToken);

            await _publisher.Publish(new AssentoReservadoNotification(
                request.EventId, request.AssentoCodigo, "Reservado"), cancellationToken);

            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure("Poxa! Outra pessoa acabou de reservar este assento na sua frente. Por favor, escolha outro.");
        }
    }
}
