using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Tests;

public class TicketTests
{
    private static readonly Guid UsuarioA = Guid.NewGuid();
    private static readonly Guid UsuarioB = Guid.NewGuid();

    // ─── Reservar ────────────────────────────────────────────────────────────

    [Fact]
    public void Reservar_QuandoDisponivel_DeveRetornarSucesso()
    {
        var ticket = new Ticket("A1");

        var resultado = ticket.Reservar(UsuarioA);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(TicketStatus.Reservado, ticket.Status);
        Assert.Equal(UsuarioA, ticket.UsuarioId);
        Assert.NotNull(ticket.DataExpiraReserva);
    }

    [Fact]
    public void Reservar_QuandoJaReservado_DeveRetornarFalha()
    {
        var ticket = new Ticket("A1");
        ticket.Reservar(UsuarioA);

        var resultado = ticket.Reservar(UsuarioB);

        Assert.False(resultado.IsSuccess);
        Assert.NotEmpty(resultado.ErrorMessage);
    }

    [Fact]
    public void Reservar_QuandoVendido_DeveRetornarFalha()
    {
        var ticket = new Ticket("A1");
        ticket.Reservar(UsuarioA);
        ticket.ConfirmarPagamento(UsuarioA);

        var resultado = ticket.Reservar(UsuarioB);

        Assert.False(resultado.IsSuccess);
    }

    // ─── ConfirmarPagamento ──────────────────────────────────────────────────

    [Fact]
    public void ConfirmarPagamento_QuandoReservadoPeloMesmoUsuario_DeveRetornarSucesso()
    {
        var ticket = new Ticket("A1");
        ticket.Reservar(UsuarioA);

        var resultado = ticket.ConfirmarPagamento(UsuarioA);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(TicketStatus.Vendido, ticket.Status);
        Assert.Null(ticket.DataExpiraReserva);
    }

    [Fact]
    public void ConfirmarPagamento_QuandoUsuarioDiferente_DeveRetornarFalha()
    {
        var ticket = new Ticket("A1");
        ticket.Reservar(UsuarioA);

        var resultado = ticket.ConfirmarPagamento(UsuarioB);

        Assert.False(resultado.IsSuccess);
        Assert.NotEmpty(resultado.ErrorMessage);
    }

    [Fact]
    public void ConfirmarPagamento_QuandoDisponivel_DeveRetornarFalha()
    {
        var ticket = new Ticket("A1");

        var resultado = ticket.ConfirmarPagamento(UsuarioA);

        Assert.False(resultado.IsSuccess);
    }

    // ─── ExpirarReserva ──────────────────────────────────────────────────────

    [Fact]
    public void ExpirarReserva_QuandoReservaVencida_DeveRetornarSucessoELiberarAssento()
    {
        var ticket = new Ticket("A1");
        ticket.Reservar(UsuarioA);

        // Força expiração: o método valida DataExpiraReserva <= UtcNow,
        // mas a data foi definida como UtcNow + 15 min; usamos reflexão para retroagir.
        typeof(Ticket)
            .GetProperty(nameof(Ticket.DataExpiraReserva))!
            .SetValue(ticket, DateTime.UtcNow.AddMinutes(-1));

        var resultado = ticket.ExpirarReserva();

        Assert.True(resultado.IsSuccess);
        Assert.Equal(TicketStatus.Disponivel, ticket.Status);
        Assert.Null(ticket.UsuarioId);
        Assert.Null(ticket.DataExpiraReserva);
    }

    [Fact]
    public void ExpirarReserva_QuandoReservaAindaValida_DeveRetornarFalha()
    {
        var ticket = new Ticket("A1");
        ticket.Reservar(UsuarioA);

        var resultado = ticket.ExpirarReserva();

        Assert.False(resultado.IsSuccess);
        Assert.Equal(TicketStatus.Reservado, ticket.Status);
    }

    [Fact]
    public void ExpirarReserva_QuandoDisponivel_DeveRetornarFalha()
    {
        var ticket = new Ticket("A1");

        var resultado = ticket.ExpirarReserva();

        Assert.False(resultado.IsSuccess);
    }

    // ─── Versao (concorrência otimista) ──────────────────────────────────────

    [Fact]
    public void Reservar_DeveAtualizarVersao()
    {
        var ticket = new Ticket("A1");
        var versaoAnterior = ticket.Versao;

        ticket.Reservar(UsuarioA);

        Assert.NotEqual(versaoAnterior, ticket.Versao);
    }
}
