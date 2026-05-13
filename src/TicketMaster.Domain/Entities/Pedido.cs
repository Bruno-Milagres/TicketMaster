namespace TicketMaster.Domain.Entities;

public enum PedidoStatus
{
    Pendente = 0,
    Pago = 1,
    Enviado = 2,
    Cancelado = 3
}

public class Pedido
{
    public Guid Id { get; private set; }
    public string? UsuarioId { get; private set; }
    public string? ClienteNome { get; private set; }
    public string? ClienteEmail { get; private set; }
    public DateTime DataPedido { get; private set; }
    public PedidoStatus Status { get; private set; }
    public decimal Total { get; private set; }

    private readonly List<ItemPedido> _itens = new();
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    private Pedido() { }

    public Pedido(string? usuarioId, string? clienteNome, string? clienteEmail)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        ClienteNome = clienteNome;
        ClienteEmail = clienteEmail;
        DataPedido = DateTime.UtcNow;
        Status = PedidoStatus.Pendente;
    }

    public void AdicionarItem(Guid tipoIngressoId, string nomeIngresso, int quantidade, decimal precoUnitario)
    {
        _itens.Add(new ItemPedido(Id, tipoIngressoId, nomeIngresso, quantidade, precoUnitario));
        Total = _itens.Sum(i => i.Subtotal);
    }

    public void AlterarStatus(PedidoStatus novoStatus) => Status = novoStatus;
}
