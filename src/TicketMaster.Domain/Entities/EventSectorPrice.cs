using TicketMaster.Domain.Enums;

namespace TicketMaster.Domain.Entities;

public class EventSectorPrice
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Sector { get; private set; }
    public TicketCategory Category { get; private set; }
    public decimal Price { get; private set; }
    public int MaxQuota { get; private set; }

    public Event? Event { get; private set; }

    private EventSectorPrice() { Sector = string.Empty; }

    public EventSectorPrice(Guid eventId, string sector, TicketCategory category, decimal price, int maxQuota)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Sector = sector;
        Category = category;
        Price = price;
        MaxQuota = maxQuota;
    }
}
