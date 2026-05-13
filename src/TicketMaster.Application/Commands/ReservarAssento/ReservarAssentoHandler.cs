using MediatR;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Notifications;
using TicketMaster.Domain.Common;
using TicketMaster.Domain.Enums;
using TicketMaster.Domain.Exceptions;

namespace TicketMaster.Application.Commands.ReservarAssento;

public sealed class ReservarAssentoHandler : IRequestHandler<ReservarAssentoCommand, Result>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPublisher _publisher;
    private readonly IQuotaService _quotaService;

    public ReservarAssentoHandler(ITicketRepository ticketRepository, IPublisher publisher, IQuotaService quotaService)
    {
        _ticketRepository = ticketRepository;
        _publisher = publisher;
        _quotaService = quotaService;
    }

    public async Task<Result> Handle(ReservarAssentoCommand request, CancellationToken cancellationToken)
    {
        // D2 — Valida cota de meia-entrada (Lei 12.933/2013)
        if (request.Category == TicketCategory.Meia)
        {
            var quotaResult = await _quotaService.VerificarMeiaEntradaAsync(request.EventId, request.AssentoCodigo, cancellationToken);
            if (!quotaResult.IsSuccess)
                return quotaResult;
        }

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

            if (request.Category == TicketCategory.Meia)
                await _quotaService.IncrementarMeiaEntradaAsync(request.EventId, request.AssentoCodigo, cancellationToken);

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
