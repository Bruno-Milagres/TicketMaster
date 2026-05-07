using Moq;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Tests;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _repositoryMock;
    private readonly TicketService _service;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public TicketServiceTests()
    {
        _repositoryMock = new Mock<ITicketRepository>();
        _service = new TicketService(_repositoryMock.Object);
    }

    [Fact]
    public async Task ReservarAssento_QuandoTudoOk_DeveChamarAtualizarNoRepositorio()
    {
        // ARRANGE (Configura o "fingimento")
        var ticket = new Ticket(Guid.NewGuid(), "A1");
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("A1", It.IsAny<Guid>())).ReturnsAsync(ticket);

        // ACT (Executa o serviço real)
        var resultado = await _service.ReservarAssentoAsync("A1", _usuarioId, Guid.NewGuid());

        // ASSERT (Verifica se o serviço se comportou bem)
        Assert.True(resultado.IsSuccess);
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task ReservarAssento_QuandoAssentoNaoExiste_DeveRetornarFalha()
    {
        // ARRANGE: O repositório retorna null
        _repositoryMock.Setup(r => r.ObterPorAssentoAsync("Z9", It.IsAny<Guid>())).ReturnsAsync((Ticket)null!);

        // ACT
        var resultado = await _service.ReservarAssentoAsync("Z9", _usuarioId, Guid.NewGuid());

        // ASSERT
        Assert.False(resultado.IsSuccess);
        Assert.Contains("não encontrado", resultado.ErrorMessage);
    }
}