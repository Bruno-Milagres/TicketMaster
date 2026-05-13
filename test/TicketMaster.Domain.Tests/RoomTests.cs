using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Tests;

public class RoomTests
{
    [Fact]
    public void Construtor_DeveInicializarCorretamente()
    {
        var layout = new Room.RoomLayout
        {
            MaxColumns = 5,
            MaxRows = 5,
            Seats = new List<Room.SeatCoordinate>
            {
                new() { SeatCode = "A1", CoordX = 1, CoordY = 1 }
            }
        };

        var sala = new Room("Sala Principal", layout);

        Assert.NotEqual(Guid.Empty, sala.Id);
        Assert.Equal("Sala Principal", sala.Name);
        Assert.NotNull(sala.Layout);
        Assert.Single(sala.Layout.Seats);
    }

    [Fact]
    public void Construtor_QuandoNomeVazio_DeveLancarArgumentException()
    {
        var layout = new Room.RoomLayout { MaxColumns = 1, MaxRows = 1 };

        var ex = Assert.Throws<ArgumentException>(() =>
            new Room("", layout));

        Assert.Contains("nome", ex.Message.ToLower());
    }

    [Fact]
    public void Construtor_QuandoLayoutNulo_DeveLancarArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Room("Sala", null!));

        Assert.Contains("layout", ex.Message.ToLower());
    }
}
