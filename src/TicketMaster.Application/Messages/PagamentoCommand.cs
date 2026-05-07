namespace TicketMaster.Application.Messages;

// Uso 'record' porque mensagens de fila devem ser imutáveis.
// Uma vez que o pedido de pagamento foi feito, ninguém pode alterar os dados no meio do caminho.
public record PagamentoCommand(string AssentoCodigo, Guid UsuarioId);