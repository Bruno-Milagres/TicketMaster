using TicketMaster.Domain.Common;
using TicketMaster.Domain.Enums;

namespace TicketMaster.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public DateTime EventDate { get; private set; }
    public Guid RoomId { get; private set; }
    public EventStatus Status { get; private set; }
    public string? ImagemUrl { get; private set; }

    //============================================================================
    // Construtor privado para o EF
    //============================================================================
    //============================================================================
    // Construtor privado para o EF Core (materialização via reflexão)
    //============================================================================
    private Event() { Title = string.Empty; }

    public Event(string title, DateTime eventDate, Guid roomId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título do evento não pode ser vazio.", nameof(title));

        if (eventDate <= DateTime.UtcNow)
            throw new ArgumentException("A data do evento deve ser futura.", nameof(eventDate));

        if (roomId == Guid.Empty)
            throw new ArgumentException("A sala do evento é obrigatória.", nameof(roomId));

        Id = Guid.NewGuid();
        Title = title;
        EventDate = eventDate;
        RoomId = roomId;
        Status = EventStatus.Rascunho;
    }

    //============================================================================
    // Publica o evento, tornando-o visível para reserva de ingressos.
    //============================================================================
    public Result Publicar()
    {
        if (Status != EventStatus.Rascunho)
            return Result.Failure("Apenas eventos em rascunho podem ser publicados.");

        Status = EventStatus.Publicado;
        return Result.Success();
    }

    //============================================================================
    // Cancela o evento. Só pode ser cancelado se estiver publicado.
    //============================================================================
    public Result Cancelar()
    {
        if (Status != EventStatus.Publicado)
            return Result.Failure("Apenas eventos publicados podem ser cancelados.");

        Status = EventStatus.Cancelado;
        return Result.Success();
    }

    public void DefinirImagem(string imagemUrl) => ImagemUrl = imagemUrl;
}
