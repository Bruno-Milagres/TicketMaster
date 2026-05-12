using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TicketMaster.Application.Services;
using TicketMaster.Web.Models;

namespace TicketMaster.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly EventService _eventService;

    public HomeController(EventService eventService)
    {
        _eventService = eventService;
    }

    //========================================================================================================================
    // Redireciona para a lista de eventos ativos
    //========================================================================================================================
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var eventos = await _eventService.ListarEventosAtivosAsync(cancellationToken);
        return View(eventos);
    }
}
