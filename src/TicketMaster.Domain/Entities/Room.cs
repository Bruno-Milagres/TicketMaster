namespace TicketMaster.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public RoomLayout Layout { get; private set; } = null!;

    /// <summary>
    /// Construtor privado exigido pelo EF Core para materialização via reflexão.
    /// </summary>
    private Room() { }

    /// <summary>
    /// Cria uma nova sala com o nome e layout informados.
    /// </summary>
    public Room(string name, RoomLayout layout)
    {
        Id = Guid.NewGuid();
        Name = name;
        Layout = layout;
    }

    /// <summary>
    /// Representa a coordenada de uma única cadeira no mapa da sala.
    /// </summary>
    public class SeatCoordinate
    {
        /// <summary>Código único do assento, por exemplo "A1".</summary>
        public string SeatCode { get; set; } = string.Empty;
        /// <summary>Coluna no CSS Grid.</summary>
        public int CoordX { get; set; }
        /// <summary>Linha no CSS Grid.</summary>
        public int CoordY { get; set; }
        /// <summary>Tipo do assento: Standard, VIP ou Cadeirante.</summary>
        public string Type { get; set; } = "Standard";
    }

    /// <summary>
    /// Representa o mapa completo da sala, com dimensões e posições de todos os assentos.
    /// </summary>
    public class RoomLayout
    {
        public int MaxColumns { get; set; }
        public int MaxRows { get; set; }
        public List<SeatCoordinate> Seats { get; set; } = new();
    }
}