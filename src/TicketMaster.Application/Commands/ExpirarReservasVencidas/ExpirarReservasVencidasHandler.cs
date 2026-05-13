using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Notifications;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Commands.ExpirarReservasVencidas;

public sealed class ExpirarReservasVencidasHandler : IRequestHandler<ExpirarReservasVencidasCommand, List<string>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPublisher _publisher;

    public ExpirarReservasVencidasHandler(ITicketRepository ticketRepository, IPublisher publisher)
    {
        _ticketRepository = ticketRepository;
        _publisher = publisher;
    }

    public async Task<List<string>> Handle(ExpirarReservasVencidasCommand request, CancellationToken cancellationToken)
    {
        var ingressosVencidos = await _ticketRepository.ObterReservasVencidasAsync(cancellationToken);
        var assentosLiberados = new List<string>();

        foreach (var ticket in ingressosVencidos)
        {
            var resultado = ticket.ExpirarReserva();

            if (resultado.IsSuccess)
            {
                await _ticketRepository.AtualizarAsync(ticket, cancellationToken);

                // Busca o EventId para a notificação
                var eventId = ticket.EventId;
                assentosLiberados.Add(ticket.AssentoCodigo);

                await _publisher.Publish(new AssentoLiberadoNotification(
                    eventId, ticket.AssentoCodigo, "Livre"), cancellationToken);
            }
        }

        return assentosLiberados;
    }
}
