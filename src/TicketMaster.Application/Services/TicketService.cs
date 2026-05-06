using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Services;

public class TicketService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    //==================================================================
    // Reserva de Assento - Fluxo Completo (Infraestrutura + Domínio)
    //==================================================================
    public async Task<Result> ReservarAssentoAsync(string assentoCodigo, Guid usuarioId)
    {
        // R1: Pede para a Infraestrutura buscar o dado no banco
        var ticket = await _ticketRepository.ObterPorAssentoAsync(assentoCodigo);

        if (ticket == null)
            return Result.Failure("Assento não encontrado no sistema.");

        // R2: Pede para o Domínio executar a Regra de Negócio (em memória)
        var resultadoReserva = ticket.Reservar(usuarioId);

        // Se a regra de negocio falhar (ex: ingresso já estava vendido), paramos por aqui.
        if (!resultadoReserva.IsSuccess)
            return resultadoReserva;

        try
        {
            // R3: Se deu tudo certo no dominio, manda a Infraestrutura salvar o novo estado no banco
            await _ticketRepository.AtualizarAsync(ticket);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("Poxa! Outra pessoa acabou de reservar este assento na sua frente. Por favor, escolha outro.");
        }
    }

    //====================================================================================
    // Expiração de Reservas Vencidas - Fluxo Completo (Infraestrutura + Domínio)
    //====================================================================================
    public async Task ExpirarReservasVencidasAsync()
    {
        var ingressosVencidos = await _ticketRepository.ObterReservasVencidasAsync();

        foreach (var ticket in ingressosVencidos)
        {
            // 1. Chama a regra de negócio do Domínio (que você criou lindamente)
            var resultado = ticket.ExpirarReserva();

            if (resultado.IsSuccess)
            {
                // 2. Manda o repositório salvar o novo status e a nova "Versao" (Guid) no banco
                await _ticketRepository.AtualizarAsync(ticket);
            }
        }
    }
}