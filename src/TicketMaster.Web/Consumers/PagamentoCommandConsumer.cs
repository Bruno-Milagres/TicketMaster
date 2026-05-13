using MassTransit;
using MediatR;
using TicketMaster.Application.Commands.ConfirmarPagamento;
using TicketMaster.Application.Messages;

namespace TicketMaster.Web.Consumers;

//========================================================================================================
// Consumidor MassTransit responsável por processar comandos de pagamento recebidos do RabbitMQ.
// Confirma o pagamento via MediatR e a notificação SignalR é feita pelo AssentoVendidoEventHandler.
//========================================================================================================
public class PagamentoCommandConsumer : IConsumer<PagamentoCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PagamentoCommandConsumer> _logger;

    public PagamentoCommandConsumer(IMediator mediator, ILogger<PagamentoCommandConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    //========================================================================================================
    // Processa um <see cref="PagamentoCommand"/> da fila:
    // confirma o pagamento via MediatR e a notificação SignalR é enviada pelo event handler.
    //========================================================================================================
    public async Task Consume(ConsumeContext<PagamentoCommand> context)
    {
        var comando = context.Message;
        _logger.LogInformation("Processando pagamento do assento {AssentoCodigo} do evento {EventId}",
            comando.AssentoCodigo, comando.EventId);

        var resultado = await _mediator.Send(
            new ConfirmarPagamentoCommand(comando.AssentoCodigo, comando.UsuarioId, comando.EventId),
            context.CancellationToken);

        if (resultado.IsSuccess)
        {
            _logger.LogInformation("Assento {AssentoCodigo} confirmado como vendido.", comando.AssentoCodigo);
        }
        else
        {
            _logger.LogWarning("Falha ao processar pagamento do assento {AssentoCodigo}: {Erro}",
                comando.AssentoCodigo, resultado.ErrorMessage);
        }
    }
}
