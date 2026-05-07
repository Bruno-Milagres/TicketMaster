using Moq;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;
using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Exceptions;

namespace TicketMaster.Domain.Tests;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _repositoryMock;
    private readonly TicketService _service;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _eventId = Guid.NewGuid();

    public TicketServiceTests()
    {
        _repositoryMock = new Mock<ITicketRepository>();
        _service = new TicketService(_repositoryMock.Object);
    }

    #region ReservarAssentoAsync

    [Fact]
    public async Task ReservarAssento_QuandoTudoOk_DeveChamarAtualizarNoRepositorio()
    {
        // Arrange
        var ticket = new Ticket(_eventId, "A1");
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("A1", _eventId)).ReturnsAsync(ticket);

        // Act
        var resultado = await _service.ReservarAssentoAsync("A1", _usuarioId, _eventId);

        // Assert
        Assert.True(resultado.IsSuccess);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task ReservarAssento_QuandoAssentoNaoExiste_DeveRetornarFalha()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("Z9", _eventId)).ReturnsAsync((Ticket)null!);

        // Act
        var resultado = await _service.ReservarAssentoAsync("Z9", _usuarioId, _eventId);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Contains("não encontrado", resultado.ErrorMessage);
    }

    [Fact]
    public async Task ReservarAssento_QuandoConcorrencia_DeveRetornarFalha()
    {
        // Arrange
        var ticket = new Ticket(_eventId, "A1");
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("A1", _eventId)).ReturnsAsync(ticket);
        _repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Ticket>()))
            .ThrowsAsync(new ConcurrencyException("Conflito"));

        // Act
        var resultado = await _service.ReservarAssentoAsync("A1", _usuarioId, _eventId);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.NotEmpty(resultado.ErrorMessage);
    }

    #endregion

    #region ConfirmarPagamentoAsync

    [Fact]
    public async Task ConfirmarPagamento_QuandoTudoOk_DeveChamarAtualizarNoRepositorio()
    {
        // Arrange
        var ticket = new Ticket(_eventId, "A1");
        ticket.Reservar(_usuarioId);
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("A1", _eventId)).ReturnsAsync(ticket);

        // Act
        var resultado = await _service.ConfirmarPagamentoAsync("A1", _usuarioId, _eventId);

        // Assert
        Assert.True(resultado.IsSuccess);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmarPagamento_QuandoAssentoNaoExiste_DeveRetornarFalha()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("Z9", _eventId)).ReturnsAsync((Ticket)null!);

        // Act
        var resultado = await _service.ConfirmarPagamentoAsync("Z9", _usuarioId, _eventId);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Contains("não encontrado", resultado.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmarPagamento_QuandoConcorrencia_DeveRetornarFalha()
    {
        // Arrange
        var ticket = new Ticket(_eventId, "A1");
        ticket.Reservar(_usuarioId);
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("A1", _eventId)).ReturnsAsync(ticket);
        _repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Ticket>()))
            .ThrowsAsync(new ConcurrencyException("Conflito"));

        // Act
        var resultado = await _service.ConfirmarPagamentoAsync("A1", _usuarioId, _eventId);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.NotEmpty(resultado.ErrorMessage);
    }

    #endregion

    #region ExpirarReservasVencidasAsync

    [Fact]
    public async Task ExpirarReservasVencidas_QuandoHaReservasVencidas_DeveLiberarERetornarAssentos()
    {
        // Arrange
        var ticket = new Ticket(_eventId, "B2");
        ticket.Reservar(_usuarioId);
        typeof(Ticket)
            .GetProperty(nameof(Ticket.DataExpiraReserva))!
            .SetValue(ticket, DateTime.UtcNow.AddMinutes(-1));

        _repositoryMock.Setup(r => r.ObterReservasVencidasAsync())
            .ReturnsAsync(new List<Ticket> { ticket });

        // Act
        var assentosLiberados = await _service.ExpirarReservasVencidasAsync();

        // Assert
        Assert.Single(assentosLiberados);
        Assert.Equal("B2", assentosLiberados[0]);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task ExpirarReservasVencidas_QuandoNaoHaReservasVencidas_DeveRetornarListaVazia()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ObterReservasVencidasAsync())
            .ReturnsAsync(new List<Ticket>());

        // Act
        var assentosLiberados = await _service.ExpirarReservasVencidasAsync();

        // Assert
        Assert.Empty(assentosLiberados);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Ticket>()), Times.Never);
    }

    #endregion

    #region ObterTodosAsync

    [Fact]
    public async Task ObterTodos_DeveRetornarTodosOsIngressos()
    {
        // Arrange
        var tickets = new List<Ticket>
        {
            new Ticket(_eventId, "A1"),
            new Ticket(_eventId, "A2"),
        };
        _repositoryMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(tickets);

        // Act
        var resultado = await _service.ObterTodosAsync();

        // Assert
        Assert.Equal(2, resultado.Count());
    }

    #endregion
}
