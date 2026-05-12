using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Notifications;
using TicketMaster.Domain.Common;
using TicketMaster.Domain.Exceptions;

namespace TicketMaster.Application.Commands.ConfirmarPagamento;

public sealed class ConfirmarPagamentoHandler : IRequestHandler<ConfirmarPagamentoCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPublisher _publisher;

    public ConfirmarPagamentoHandler(ITicketRepository ticketRepository, IPublisher publisher)
    {
        _ticketRepository = ticketRepository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(ConfirmarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(
            request.AssentoCodigo, request.EventId, cancellationToken);

        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");

        var resultado = ticket.ConfirmarPagamento(request.UsuarioId);

        if (!resultado.IsSuccess)
            return resultado;

        try
        {
            await _ticketRepository.AtualizarAsync(ticket, cancellationToken);

            await _publisher.Publish(new AssentoVendidoNotification(
                request.EventId, request.AssentoCodigo, "Vendido"), cancellationToken);

            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure("Poxa! Parece que houve um problema ao confirmar seu pagamento. Por favor, tente novamente.");
        }
    }
}
