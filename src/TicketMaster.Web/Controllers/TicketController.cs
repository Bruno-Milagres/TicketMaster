using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TicketMaster.Application.Messages;
using TicketMaster.Application.Services;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.Controllers;

public class TicketController : Controller
{
    private readonly TicketService _ticketService;
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public TicketController(TicketService ticketService, IHubContext<TicketHub> hubContext, IPublishEndpoint publishEndpoint)
    {
        _ticketService = ticketService;
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tickets = await _ticketService.ObterTodosAsync();
        return View(tickets);
    }

    /// <summary>
    /// Redireciona para a listagem de ingressos caso o usuário chegue via GET
    /// após ser redirecionado pelo fluxo de login.
    /// </summary>
    [HttpGet]
    public IActionResult Reservar()
    {
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Reservar(string assentoCodigo, Guid eventId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _ticketService.ReservarAssentoAsync(assentoCodigo, usuarioLogadoId, eventId);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction("Index");
        }

        await _hubContext.Clients.All.SendAsync("AtualizarAssento", assentoCodigo, "Reservado");
        return RedirectToAction("Checkout", new { codigo = assentoCodigo });
    }

    [HttpGet]
    public IActionResult Pagar()
    {
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Pagar(string assentoCodigo, Guid eventId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var comando = new PagamentoCommand(assentoCodigo, usuarioLogadoId, eventId);

        await _publishEndpoint.Publish(comando);

        TempData["Sucesso"] = $"Seu pedido de pagamento para o assento {assentoCodigo} foi para a fila. Aguarde a confirmação no mapa!";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Checkout(string codigo)
    {
        return View((object)codigo);
    }
}