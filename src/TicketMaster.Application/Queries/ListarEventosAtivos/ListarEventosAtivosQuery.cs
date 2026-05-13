using MediatR;
using TicketMaster.Application.Common;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Queries.ListarEventosAtivos;

public sealed record ListarEventosAtivosQuery(int Pagina = 1, int TamanhoPagina = 12)
    : IRequest<PagedResult<Event>>;
