namespace TicketMaster.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public RoomLayout Layout { get; private set; } = null!;

    // R1. Construtor privado para o EF Core (ele consegue preencher as propriedades via Reflection)
    private Room() { }

    // R2. Seu construtor público que o resto do sistema vai usar
    public Room(string name, RoomLayout layout)
    {
        Id = Guid.NewGuid();
        Name = name;
        Layout = layout;
    }

    // Representa a coordenada de uma única cadeira no mapa
    public class SeatCoordinate
    {
        public string SeatCode { get; set; } = string.Empty; // Ex: "A1"
        public int CoordX { get; set; } // Coluna no CSS Grid
        public int CoordY { get; set; } // Linha no CSS Grid
        public string Type { get; set; } = "Standard"; // Ex: Standard, VIP, Cadeirante
    }

    // Representa o mapa completo da sala
    public class RoomLayout
    {
        public int MaxColumns { get; set; }
        public int MaxRows { get; set; }
        public List<SeatCoordinate> Seats { get; set; } = new();
    }
}