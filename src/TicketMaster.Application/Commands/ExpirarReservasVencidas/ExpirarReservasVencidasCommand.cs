using MediatR;

namespace TicketMaster.Application.Commands.ExpirarReservasVencidas;

public sealed record ExpirarReservasVencidasCommand : IRequest<List<string>>;
