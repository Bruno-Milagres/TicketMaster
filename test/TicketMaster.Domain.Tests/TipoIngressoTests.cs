using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Tests;

public class TipoIngressoTests
{
    private readonly Guid _eventId = Guid.NewGuid();

    [Fact]
    public void Construtor_DeveInicializarCorretamente()
    {
        var tipo = new TipoIngresso(_eventId, "Inteira", 100m, 200);

        Assert.NotEqual(Guid.Empty, tipo.Id);
        Assert.Equal("Inteira", tipo.Nome);
        Assert.Equal(100m, tipo.Preco);
        Assert.Equal(200, tipo.QuantidadeDisponivel);
    }

    [Fact]
    public void Atualizar_DeveAlterarPropriedades()
    {
        var tipo = new TipoIngresso(_eventId, "Inteira", 100m, 200);

        tipo.Atualizar("Meia", 50m, 150);

        Assert.Equal("Meia", tipo.Nome);
        Assert.Equal(50m, tipo.Preco);
        Assert.Equal(150, tipo.QuantidadeDisponivel);
    }

    [Fact]
    public void EstaDisponivel_QuandoQuantidadeSuficiente_DeveRetornarTrue()
    {
        var tipo = new TipoIngresso(_eventId, "Inteira", 100m, 10);

        Assert.True(tipo.EstaDisponivel(5));
        Assert.True(tipo.EstaDisponivel(10));
    }

    [Fact]
    public void EstaDisponivel_QuandoQuantidadeInsuficiente_DeveRetornarFalse()
    {
        var tipo = new TipoIngresso(_eventId, "Inteira", 100m, 10);

        Assert.False(tipo.EstaDisponivel(11));
        Assert.False(tipo.EstaDisponivel(100));
    }

    [Fact]
    public void ReservarEstoque_DeveDiminuirQuantidade()
    {
        var tipo = new TipoIngresso(_eventId, "Inteira", 100m, 50);

        tipo.ReservarEstoque(10);

        Assert.Equal(40, tipo.QuantidadeDisponivel);
    }

    [Fact]
    public void LiberarEstoque_DeveAumentarQuantidade()
    {
        var tipo = new TipoIngresso(_eventId, "Inteira", 100m, 50);
        tipo.ReservarEstoque(10);

        tipo.LiberarEstoque(5);

        Assert.Equal(45, tipo.QuantidadeDisponivel);
    }
}
