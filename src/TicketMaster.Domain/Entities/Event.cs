namespace TicketMaster.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public DateTime EventDate { get; private set; }
    public Guid RoomId { get; private set; }

    //============================================================================
    // Contrutor privado para o EF
    //============================================================================
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
    }
}