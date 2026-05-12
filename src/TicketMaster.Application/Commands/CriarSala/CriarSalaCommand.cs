using MediatR;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Commands.CriarSala;

public sealed record CriarSalaCommand(string Nome, Room.RoomLayout Layout) : IRequest<Guid>;
