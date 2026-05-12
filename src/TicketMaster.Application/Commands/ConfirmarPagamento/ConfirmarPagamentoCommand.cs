using MediatR;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Commands.ConfirmarPagamento;

public sealed record ConfirmarPagamentoCommand(
    string AssentoCodigo,
    Guid UsuarioId,
    Guid EventId
) : IRequest<Result>;
