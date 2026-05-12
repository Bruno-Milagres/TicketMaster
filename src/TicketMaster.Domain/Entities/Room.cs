namespace TicketMaster.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public RoomLayout Layout { get; private set; } = null!;

    //============================================================================
    // Construtor privado exigido pelo EF Core para materialização via reflexão.
    //============================================================================
    private Room() { }

    //============================================================================
    // Cria uma nova sala com o nome e layout informados.
    //============================================================================
    public Room(string name, RoomLayout layout)
    {
        Id = Guid.NewGuid();
        Name = name;
        Layout = layout;
    }

    //============================================================================
    // Representa a coordenada de uma única cadeira no mapa da sala.
    //============================================================================
    public class SeatCoordinate
    {
        // Código único do assento, por exemplo "A1".
        public string SeatCode { get; set; } = string.Empty;
        //Coluna no CSS Grid./
        public int CoordX { get; set; }
        // Linha no CSS Grid.
        public int CoordY { get; set; }
        // Tipo do assento: Standard, VIP ou Cadeirante.
        public string Type { get; set; } = "Standard";
    }

    //==========================================================================================
    // Representa o mapa completo da sala, com dimensões e posições de todos os assentos.
    //==========================================================================================
    public class RoomLayout
    {
        public int MaxColumns { get; set; }
        public int MaxRows { get; set; }
        public List<SeatCoordinate> Seats { get; set; } = new();
    }
}