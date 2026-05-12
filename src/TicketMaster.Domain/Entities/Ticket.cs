using TicketMaster.Domain.Common;

namespace TicketMaster.Domain.Entities;

public enum TicketStatus
{
    Disponivel = 0,
    Reservado = 1,
    Vendido = 2
}

public sealed class Ticket
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; } 
    public string AssentoCodigo { get; private set; }
    public TicketStatus Status { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public DateTime? DataExpiraReserva { get; private set; }
    public Guid Versao { get; private set; }

    //============================================================================
    // Construtor privado exigido pelo EF Core para materialização via reflexão.
    //============================================================================
    public Ticket(Guid eventId, string assentoCodigo)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        AssentoCodigo = assentoCodigo;
        Status = TicketStatus.Disponivel;
        Versao = Guid.NewGuid();
    }

    //============================================================================
    // Cria uma reservar para o ingresso, associando-o a um usuário e
    // definindo um prazo de expiração para a reserva.
    //============================================================================
    public Result Reservar(Guid usuarioId)
    {
        if (Status != TicketStatus.Disponivel)
            return Result.Failure("O ingresso não está disponível para reserva.");

        Status = TicketStatus.Reservado;
        UsuarioId = usuarioId;
        DataExpiraReserva = DateTime.UtcNow.AddMinutes(15);
        Versao = Guid.NewGuid();

        return Result.Success();
    }

    //============================================================================
    // Confirma o pagamento do ingresso reservado, alterando seu status para vendido.
    //============================================================================
    public Result ConfirmarPagamento(Guid usuarioId)
    {
        if (Status != TicketStatus.Reservado)
            return Result.Failure("O ingresso deve estar reservado para confirmar o pagamento.");

        if (UsuarioId != usuarioId)
            return Result.Failure("O usuário que confirmou o pagamento deve ser o mesmo que reservou o ingresso.");

        Status = TicketStatus.Vendido;
        DataExpiraReserva = null;
        Versao = Guid.NewGuid();

        return Result.Success();
    }

    //============================================================================
    // Expira a reserva de um ingresso, tornando-o disponível novamente para outros usuários.
    //============================================================================
    public Result ExpirarReserva()
    {
        if (Status != TicketStatus.Reservado)
            return Result.Failure("O ingresso deve estar reservado para expirar a reserva.");

        if (DataExpiraReserva.HasValue && DateTime.UtcNow < DataExpiraReserva.Value)
            return Result.Failure("A reserva ainda está dentro do prazo de validade.");

        Status = TicketStatus.Disponivel;
        UsuarioId = null;
        DataExpiraReserva = null;
        Versao = Guid.NewGuid();

        return Result.Success();
    }

    //============================================================================
    // Permite que o usuário cancele sua própria reserva, tornando o ingresso disponível novamente.
    //============================================================================
    public Result CancelarReservaPeloUsuario(Guid usuarioId)
    {
        if (Status != TicketStatus.Reservado)
            return Result.Failure("Este ingresso não está reservado.");

        if (UsuarioId != usuarioId)
            return Result.Failure("Você só pode cancelar a sua própria reserva.");

        Status = TicketStatus.Disponivel;
        UsuarioId = null;
        DataExpiraReserva = null;

        return Result.Success();
    }
}
