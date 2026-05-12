namespace TicketMaster.Domain.Entities;

public class ItemPedido
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Guid TipoIngressoId { get; private set; }
    public string NomeIngresso { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;

    public Pedido Pedido { get; private set; } = null!;

    private ItemPedido() { NomeIngresso = string.Empty; }

    public ItemPedido(Guid pedidoId, Guid tipoIngressoId, string nomeIngresso, int quantidade, decimal precoUnitario)
    {
        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        TipoIngressoId = tipoIngressoId;
        NomeIngresso = nomeIngresso;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }
}
