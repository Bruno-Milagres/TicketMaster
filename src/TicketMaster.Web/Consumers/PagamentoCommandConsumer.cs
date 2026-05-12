using MassTransit;
using Microsoft.AspNetCore.SignalR;
using TicketMaster.Application.Messages;
using TicketMaster.Application.Services;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.Consumers;

//========================================================================================================
// Consumidor MassTransit responsável por processar comandos de pagamento recebidos do RabbitMQ.
// Confirma o pagamento no domínio e notifica os clientes conectados via SignalR.
//========================================================================================================
public class PagamentoCommandConsumer : IConsumer<PagamentoCommand>
{
    private readonly TicketService _ticketService;
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly ILogger<PagamentoCommandConsumer> _logger;

    public PagamentoCommandConsumer(
        TicketService ticketService,
        IHubContext<TicketHub> hubContext,
        ILogger<PagamentoCommandConsumer> logger)
    {
        _ticketService = ticketService;
        _hubContext = hubContext;
        _logger = logger;
    }

    //========================================================================================================
    // Processa um <see cref="PagamentoCommand"/> da fila:
    // confirma o pagamento e, em caso de sucesso, notifica apenas os clientes
    // do grupo do evento correspondente via SignalR.
    //========================================================================================================
    public async Task Consume(ConsumeContext<PagamentoCommand> context)
    {
        var comando = context.Message;
        _logger.LogInformation("Processando pagamento do assento {AssentoCodigo} do evento {EventId}",
            comando.AssentoCodigo, comando.EventId);

        var resultado = await _ticketService.ConfirmarPagamentoAsync(
            comando.AssentoCodigo,
            comando.UsuarioId,
            comando.EventId,
            context.CancellationToken);

        if (resultado.IsSuccess)
        {
            await _hubContext.Clients.Group(comando.EventId.ToString())
                .SendAsync("AtualizarAssento", comando.AssentoCodigo, "Vendido");

            _logger.LogInformation("Assento {AssentoCodigo} confirmado como vendido.", comando.AssentoCodigo);
        }
        else
        {
            _logger.LogWarning("Falha ao processar pagamento do assento {AssentoCodigo}: {Erro}",
                comando.AssentoCodigo, resultado.ErrorMessage);
        }
    }
}