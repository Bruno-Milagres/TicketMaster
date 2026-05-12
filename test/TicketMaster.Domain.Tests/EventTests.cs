using TicketMaster.Domain.Entities;
using TicketMaster.Domain.Enums;

namespace TicketMaster.Domain.Tests;

public class EventTests
{
    [Fact]
    public void Construtor_DeveInicializarComoRascunho()
    {
        var evento = new Event("Show Teste", DateTime.UtcNow.AddDays(7), Guid.NewGuid());

        Assert.Equal(EventStatus.Rascunho, evento.Status);
        Assert.NotEqual(Guid.Empty, evento.Id);
    }

    [Fact]
    public void Construtor_QuandoTituloVazio_DeveLancarArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Event("", DateTime.UtcNow.AddDays(7), Guid.NewGuid()));

        Assert.Contains("título", ex.Message.ToLower());
    }

    [Fact]
    public void Construtor_QuandoDataPassada_DeveLancarArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Event("Show", DateTime.UtcNow.AddDays(-1), Guid.NewGuid()));

        Assert.Contains("data", ex.Message.ToLower());
    }

    [Fact]
    public void Construtor_QuandoRoomIdVazio_DeveLancarArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Event("Show", DateTime.UtcNow.AddDays(7), Guid.Empty));

        Assert.Contains("sala", ex.Message.ToLower());
    }

    [Fact]
    public void Publicar_QuandoRascunho_DeveAlterarParaPublicado()
    {
        var evento = new Event("Show", DateTime.UtcNow.AddDays(7), Guid.NewGuid());

        var resultado = evento.Publicar();

        Assert.True(resultado.IsSuccess);
        Assert.Equal(EventStatus.Publicado, evento.Status);
    }

    [Fact]
    public void Publicar_QuandoJaPublicado_DeveRetornarFalha()
    {
        var evento = new Event("Show", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        evento.Publicar();

        var resultado = evento.Publicar();

        Assert.False(resultado.IsSuccess);
        Assert.Contains("rascunho", resultado.ErrorMessage.ToLower());
    }

    [Fact]
    public void Publicar_QuandoCancelado_DeveRetornarFalha()
    {
        var evento = new Event("Show", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        evento.Publicar();
        evento.Cancelar();

        var resultado = evento.Publicar();

        Assert.False(resultado.IsSuccess);
        Assert.Contains("rascunho", resultado.ErrorMessage.ToLower());
    }

    [Fact]
    public void Cancelar_QuandoPublicado_DeveAlterarParaCancelado()
    {
        var evento = new Event("Show", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        evento.Publicar();

        var resultado = evento.Cancelar();

        Assert.True(resultado.IsSuccess);
        Assert.Equal(EventStatus.Cancelado, evento.Status);
    }

    [Fact]
    public void Cancelar_QuandoRascunho_DeveRetornarFalha()
    {
        var evento = new Event("Show", DateTime.UtcNow.AddDays(7), Guid.NewGuid());

        var resultado = evento.Cancelar();

        Assert.False(resultado.IsSuccess);
        Assert.Contains("publicados", resultado.ErrorMessage.ToLower());
    }
}
