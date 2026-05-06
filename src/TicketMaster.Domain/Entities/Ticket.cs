using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMaster.Domain.Entities
{
    public enum TicketStatus
    {
        Disponivel = 0,
        Reservado = 1,
        Vendido = 2
    }

    public sealed class Ticket
    {
        public Guid Id { get; private set; }
        public string AssentoCodigo { get; private set; }
        public TicketStatus Status { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public DateTime? DataExpiraReserva { get; private set; }

        // Controle de concorrência otimista (EF Core)
        public Guid Versao { get; private set; } = Guid.NewGuid();

        public Ticket(string assentoCodigo)
        {
            Id = Guid.NewGuid();
            AssentoCodigo = assentoCodigo;
            Status = TicketStatus.Disponivel;
        }

        // ==========================================
        // REGRA DE NEGÓCIO
        // ==========================================                                                                                                         

        public Result Reservar(Guid usuarioId)
        {
            // Fail Fast: Falha a validação de negócio o mais rápido possível
            if (Status != TicketStatus.Disponivel)
                return Result.Failure("O ingresso não está disponível para reserva.");

            // Mutações de Estado
            Status = TicketStatus.Reservado;
            UsuarioId = usuarioId;
            DataExpiraReserva = DateTime.UtcNow.AddMinutes(15);

            // Atualiza a versão para controle de concorrência otimista
            Versao = Guid.NewGuid();

            return Result.Success();
        }

        public Result ConfirmarPagamento(Guid usarioId)
        {
            if (Status != TicketStatus.Reservado)
                return Result.Failure("O ingresso deve estar reservado para confirmar o pagamento.");
            if (UsuarioId != usarioId)
                return Result.Failure("O usuário que confirmou o pagamento deve ser o mesmo que reservou o ingresso.");
            Status = TicketStatus.Vendido;
            DataExpiraReserva = null; // Limpa a data de expiracao, o ticket foi vendido
            Versao = Guid.NewGuid(); // Atualiza a versão para controle de concorrência otimista
            return Result.Success();
        }

        public Result ExpirarReserva()
        {
            if (Status != TicketStatus.Reservado)
                return Result.Failure("O ingresso deve estar reservado para expirar a reserva.");

            if (DataExpiraReserva.HasValue && DateTime.UtcNow < DataExpiraReserva.Value)
                return Result.Failure("A reserva ainda está dentro do prazo de validade.");

            // Reverte o estado para disponível
            Status = TicketStatus.Disponivel;
            UsuarioId = null;
            DataExpiraReserva = null;
            Versao = Guid.NewGuid(); // Atualiza a versão para controle de concorrência otimista
            return Result.Success();
        }
    }

    // Classe auxiliar no mesmo arquivo por enquanto
    public class Result
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public Result(bool success, string error = "") { IsSuccess = success; ErrorMessage = error; }
        public static Result Success() => new Result(true);
        public static Result Failure(string message) => new Result(false, message);
    }
}
