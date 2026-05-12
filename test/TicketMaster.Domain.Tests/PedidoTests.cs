using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Tests;

public class PedidoTests
{
    [Fact]
    public void Construtor_DeveInicializarComoPendente()
    {
        var pedido = new Pedido(Guid.NewGuid().ToString(), "João", "joao@email.com");

        Assert.Equal(PedidoStatus.Pendente, pedido.Status);
        Assert.NotEqual(Guid.Empty, pedido.Id);
        Assert.Empty(pedido.Itens);
        Assert.Equal(0, pedido.Total);
    }

    [Fact]
    public void Construtor_QuandoSemUsuarioLogado_DeveUsarDadosDoCliente()
    {
        var pedido = new Pedido(null, "Maria", "maria@email.com");

        Assert.Null(pedido.UsuarioId);
        Assert.Equal("Maria", pedido.ClienteNome);
        Assert.Equal("maria@email.com", pedido.ClienteEmail);
    }

    [Fact]
    public void AdicionarItem_DeveCalcularTotalCorretamente()
    {
        var pedido = new Pedido(null, "João", "joao@email.com");
        var tipoIngressoId = Guid.NewGuid();

        pedido.AdicionarItem(tipoIngressoId, "Inteira", 2, 100m);
        pedido.AdicionarItem(tipoIngressoId, "Meia", 1, 50m);

        Assert.Equal(2, pedido.Itens.Count);
        Assert.Equal(250m, pedido.Total); // 2*100 + 1*50
    }

    [Fact]
    public void AdicionarItem_DeveCalcularSubtotalCorretamente()
    {
        var pedido = new Pedido(null, "João", "joao@email.com");
        var tipoIngressoId = Guid.NewGuid();

        pedido.AdicionarItem(tipoIngressoId, "VIP", 3, 200m);

        var item = pedido.Itens.First();
        Assert.Equal(3, item.Quantidade);
        Assert.Equal(200m, item.PrecoUnitario);
        Assert.Equal(600m, item.Subtotal); // 3*200
    }

    [Fact]
    public void AlterarStatus_DeveAtualizarStatus()
    {
        var pedido = new Pedido(null, "João", "joao@email.com");

        pedido.AlterarStatus(PedidoStatus.Pago);

        Assert.Equal(PedidoStatus.Pago, pedido.Status);
    }

    [Fact]
    public void AlterarStatus_DevePermitirMultiplasTransicoes()
    {
        var pedido = new Pedido(null, "João", "joao@email.com");

        pedido.AlterarStatus(PedidoStatus.Pago);
        pedido.AlterarStatus(PedidoStatus.Enviado);
        pedido.AlterarStatus(PedidoStatus.Cancelado);

        Assert.Equal(PedidoStatus.Cancelado, pedido.Status);
    }
}
