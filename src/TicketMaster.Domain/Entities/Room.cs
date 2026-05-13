namespace TicketMaster.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public RoomLayout Layout { get; private set; } = null!;

    // ==========================================================
    // Setores fixos do teatro
    // ==========================================================
    public static class Sectors
    {
        public const string PlateiaFrente  = "PlateiaFrente";
        public const string PlateiaCentro  = "PlateiaCentro";
        public const string PlateiaFundo   = "PlateiaFundo";
        public const string Frisa          = "Frisa";
        public const string Camarote       = "Camarote";
        public const string Balcao         = "Balcao";
        public const string Acessibilidade = "Acessibilidade";

        public static readonly IReadOnlyList<string> All = new[]
        {
            PlateiaFrente, PlateiaCentro, PlateiaFundo,
            Frisa, Camarote, Balcao, Acessibilidade
        };
    }

    //============================================================================
    // Construtor privado exigido pelo EF Core para materialização via reflexão.
    //============================================================================
    private Room() { }

    //============================================================================
    // Cria uma nova sala com o nome e layout informados.
    //============================================================================
    public Room(string name, RoomLayout layout)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da sala não pode ser vazio.", nameof(name));

        if (layout == null)
            throw new ArgumentNullException(nameof(layout), "O layout da sala é obrigatório.");

        Id = Guid.NewGuid();
        Name = name;
        Layout = layout;
    }

    //============================================================================
    // Representa a coordenada de uma única cadeira no mapa da sala.
    //============================================================================
    public class SeatCoordinate
    {
        /// <summary>Código único do assento, ex: "A1", "CAM-A1", "BAL-A1".</summary>
        public string SeatCode { get; set; } = string.Empty;
        /// <summary>Coluna no CSS Grid.</summary>
        public int CoordX { get; set; }
        /// <summary>Linha no CSS Grid.</summary>
        public int CoordY { get; set; }
        /// <summary>Tipo: Standard, VIP, Cadeirante.</summary>
        public string Type { get; set; } = "Standard";
        /// <summary>Setor do teatro: PlateiaFrente, PlateiaCentro, PlateiaFundo, Frisa, Camarote, Balcao, Acessibilidade.</summary>
        public string Sector { get; set; } = Sectors.PlateiaCentro;
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
