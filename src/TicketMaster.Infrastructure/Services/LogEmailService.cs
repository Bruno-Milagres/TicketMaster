using Microsoft.Extensions.Logging;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Infrastructure.Services;

public class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;

    public LogEmailService(ILogger<LogEmailService> logger) => _logger = logger;

    public Task SendOrderConfirmationAsync(string toEmail, string toName, Pedido pedido)
    {
        _logger.LogInformation(
            "[EMAIL] Pedido #{PedidoId} confirmado para {Nome} <{Email}> — Total: {Total:C2}, Itens: {Itens}",
            pedido.Id.ToString()[..8], toName, toEmail, pedido.Total, pedido.Itens.Count);
        return Task.CompletedTask;
    }
}
