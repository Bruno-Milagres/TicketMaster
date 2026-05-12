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
        Id = Guid.NewGuid();
        Title = title;
        EventDate = eventDate;
        RoomId = roomId;
    }
}