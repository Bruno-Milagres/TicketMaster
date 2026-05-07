using MassTransit;
using Microsoft.AspNetCore.SignalR;
using TicketMaster.Application.Messages;
using TicketMaster.Application.Services;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.Consumers;

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

    public async Task Consume(ConsumeContext<PagamentoCommand> context)
    {
        var comando = context.Message;
        _logger.LogInformation("Lendo da Fila: Processando pagamento do assento {AssentoCodigo} do evento {EventId}",
            comando.AssentoCodigo, comando.EventId);

        // Agora passamos o terceiro parâmetro!
        var resultado = await _ticketService.ConfirmarPagamentoAsync(
            comando.AssentoCodigo,
            comando.UsuarioId,
            comando.EventId);

        if (resultado.IsSuccess)
        {
            // MUDANÇA SENIOR: Em vez de All (todos), enviamos apenas para o Grupo do Evento!
            await _hubContext.Clients.Group(comando.EventId.ToString())
                .SendAsync("AtualizarAssento", comando.AssentoCodigo, "Vendido");

            _logger.LogInformation("Sucesso! Assento {AssentoCodigo} vendido.", comando.AssentoCodigo);
        }
        else
        {
            _logger.LogWarning("Falha ao processar pagamento do {AssentoCodigo}: {Erro}", comando.AssentoCodigo, resultado.ErrorMessage);
        }
    }
}