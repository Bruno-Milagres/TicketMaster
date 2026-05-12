using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Common;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Exceptions;

namespace TicketMaster.Application.Services;

public class TicketService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.ObterPorEventoAsync(eventId, cancellationToken);
    }

    //============================================================================================================================
    // Reserva um assento para o usuário informado.
    // Retorna falha se o assento não existir, já estiver ocupado ou houver conflito de concorrência.
    //============================================================================================================================
    public async Task<Result> ReservarAssentoAsync(string assentoCodigo, Guid usuarioId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(assentoCodigo, eventId, cancellationToken);

        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");

        var resultado = ticket.Reservar(usuarioId);

        if (!resultado.IsSuccess)
            return resultado;

        try
        {
            await _ticketRepository.AtualizarAsync(ticket, cancellationToken);
            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure("Poxa! Outra pessoa acabou de reservar este assento na sua frente. Por favor, escolha outro.");
        }
    }

    //============================================================================================================================
    // Libera todos os ingressos com reservas expiradas, devolvendo-os ao estoque.
    // Chamado periodicamente pelo <see cref="Web.Workers.TicketReaperWorker"/>.
    //============================================================================================================================
    public async Task<List<string>> ExpirarReservasVencidasAsync(CancellationToken cancellationToken = default)
    {
        var ingressosVencidos = await _ticketRepository.ObterReservasVencidasAsync(cancellationToken);
        var assentosLiberados = new List<string>();

        foreach (var ticket in ingressosVencidos)
        {
            var resultado = ticket.ExpirarReserva();

            if (resultado.IsSuccess)
            {
                await _ticketRepository.AtualizarAsync(ticket, cancellationToken);

                assentosLiberados.Add(ticket.AssentoCodigo);
            }
        }

        return assentosLiberados;
    }

    //============================================================================================================================
    // Confirma o pagamento de um ingresso reservado, associando-o ao usuário e marcando-o como vendido.
    //============================================================================================================================
    public async Task<Result> ConfirmarPagamentoAsync(string assentoCodigo, Guid usuarioId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(assentoCodigo, eventId, cancellationToken);
        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");
        var resultado = ticket.ConfirmarPagamento(usuarioId);
        if (!resultado.IsSuccess)
            return resultado;
        try
        {
            await _ticketRepository.AtualizarAsync(ticket, cancellationToken);
            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure("Poxa! Parece que houve um problema ao confirmar seu pagamento. Por favor, tente novamente.");
        }
    }

    //============================================================================================================================
    // Cancela a reserva de um ingresso pelo usuário.
    //============================================================================================================================
    public async Task<Result> CancelarReservaAsync(string assentoCodigo, Guid usuarioId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(assentoCodigo, eventId, cancellationToken);
        if (ticket == null) return Result.Failure("Assento não encontrado.");

        var resultado = ticket.CancelarReservaPeloUsuario(usuarioId);
        if (!resultado.IsSuccess) return resultado;

        await _ticketRepository.AtualizarAsync(ticket, cancellationToken);
        return Result.Success();
    }
}