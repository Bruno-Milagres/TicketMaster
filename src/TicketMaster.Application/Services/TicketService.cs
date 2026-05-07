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

    public async Task<IEnumerable<Ticket>> ObterTodosAsync()
    {
        return await _ticketRepository.ObterTodosAsync();
    }

    /// <summary>
    /// Reserva um assento para o usuário informado.
    /// Retorna falha se o assento não existir, já estiver ocupado ou houver conflito de concorrência.
    /// </summary>
    public async Task<Result> ReservarAssentoAsync(string assentoCodigo, Guid usuarioId, Guid eventId)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(assentoCodigo, eventId);

        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");

        var resultado = ticket.Reservar(usuarioId);

        if (!resultado.IsSuccess)
            return resultado;

        try
        {
            await _ticketRepository.AtualizarAsync(ticket);
            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure("Poxa! Outra pessoa acabou de reservar este assento na sua frente. Por favor, escolha outro.");
        }
    }

    /// <summary>
    /// Libera todos os ingressos com reservas expiradas, devolvendo-os ao estoque.
    /// Chamado periodicamente pelo <see cref="Web.Workers.TicketReaperWorker"/>.
    /// </summary>
    public async Task<List<string>> ExpirarReservasVencidasAsync()
    {
        var ingressosVencidos = await _ticketRepository.ObterReservasVencidasAsync();
        var assentosLiberados = new List<string>();

        foreach (var ticket in ingressosVencidos)
        {
            var resultado = ticket.ExpirarReserva();

            if (resultado.IsSuccess)
            {
                await _ticketRepository.AtualizarAsync(ticket);

                assentosLiberados.Add(ticket.AssentoCodigo);
            }
        }

        return assentosLiberados;
    }

    /// <summary>
    /// Confirma o pagamento de um ingresso reservado, associando-o ao usuário e marcando-o como vendido.
    /// </summary>
    public async Task<Result> ConfirmarPagamentoAsync(string assentoCodigo, Guid usuarioId, Guid eventId)
    {
        var ticket = await _ticketRepository.ObterPorAssentoAsync(assentoCodigo, eventId);
        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");
        var resultado = ticket.ConfirmarPagamento(usuarioId);
        if (!resultado.IsSuccess)
            return resultado;
        try
        {
            await _ticketRepository.AtualizarAsync(ticket);
            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure("Poxa! Parece que houve um problema ao confirmar seu pagamento. Por favor, tente novamente.");
        }
    }
}