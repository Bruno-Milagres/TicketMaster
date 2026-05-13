using TicketMaster.Domain.Entities;

namespace TicketMaster.Domain.Events;

public class PedidoConfirmadoEvent
{
    public Pedido Pedido { get; }
    public string? ClienteEmail { get; }
    public string? ClienteNome { get; }

    public PedidoConfirmadoEvent(Pedido pedido, string? clienteEmail, string? clienteNome)
    {
        Pedido = pedido;
        ClienteEmail = clienteEmail;
        ClienteNome = clienteNome;
    }
}
