using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketMaster.Application.Queries.ListarEventosAtivos;

namespace TicketMaster.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    //========================================================================================================================
    // Redireciona para a lista de eventos ativos
    //========================================================================================================================
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var eventos = await _mediator.Send(new ListarEventosAtivosQuery(), cancellationToken);
        return View(eventos);
    }
}
