using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string toEmail, string toName, Pedido pedido);
}
