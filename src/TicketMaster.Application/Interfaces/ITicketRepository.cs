using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces
{
    public interface ITicketRepository
    {
        // Busca um ticket por codigo do assento
        Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo);
        // Salva as alteracoes no banco
        Task AtualizarAsync(Ticket ticket);
        // Busca todos os tickets do banco (A1, A2, A3...)
        Task<IEnumerable<Ticket>> ObterTodosAsync();

        // Busca os tickets que estao reservados e a data de expiracao da reserva ja passou
        Task<IEnumerable<Ticket>> ObterReservasVencidasAsync();
    }
}
