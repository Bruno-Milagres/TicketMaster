using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketMaster.Application.Queries.ObterIngressosPorEvento;

namespace TicketMaster.Web.Controllers;

[Route("api/events/{eventId}")]
[ApiController]
public class SeatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeatsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("seats")]
    public async Task<IActionResult> GetSnapshot(Guid eventId, CancellationToken ct)
    {
        var tickets = await _mediator.Send(new ObterIngressosPorEventoQuery(eventId), ct);
        var dict = tickets.ToDictionary(
            t => t.AssentoCodigo,
            t => (int)t.Status);
        Response.Headers["Cache-Control"] = "public, max-age=2";
        return Ok(dict);
    }
}
