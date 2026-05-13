using MediatR;

namespace TicketMaster.Application.Commands.AtualizarSala;

public sealed record AtualizarSalaCommand(Guid Id, string Nome, Domain.Entities.Room.RoomLayout Layout) : IRequest;
