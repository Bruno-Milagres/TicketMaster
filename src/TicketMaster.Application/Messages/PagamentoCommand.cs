namespace TicketMaster.Application.Messages;

//=============================================================================================================
// Comando publicado na fila do RabbitMQ para solicitar a confirmação do pagamento de um ingresso.
// Modelado como <c>record</c> para garantir imutabilidade: após enviado, seus dados não podem ser alterados.
//=============================================================================================================
public record PagamentoCommand(string AssentoCodigo, Guid UsuarioId, Guid EventId);