namespace TicketMaster.Domain.Entities;

public class CamaroteGroup
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Nome { get; private set; }

    private readonly List<Ticket> _assentos = new();
    public IReadOnlyCollection<Ticket> Assentos => _assentos.AsReadOnly();

    public Event? Event { get; private set; }

    private CamaroteGroup() { Nome = string.Empty; }

    public CamaroteGroup(Guid eventId, string nome)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Nome = nome;
    }
}
