using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TicketMaster.Application.Messages;
using TicketMaster.Application.Services;
using TicketMaster.Web.Hubs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

    //==================================================================
    // O PARA-QUEDAS DO LOGIN
    // Se o usuario for redirecionado para ca via GET apas o login
    //==================================================================
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

        //R1. Mensagem
        var comando = new PagamentoCommand(assentoCodigo, usuarioLogadoId, eventId);

        //R2. Joga na Fila do RabbitMQ
        await _publishEndpoint.Publish(comando);

        // R3. Devolve a tela pro usuario na hora
        TempData["Sucesso"] = $"Seu pedido de pagamento para o assento {assentoCodigo} foi para a fila. Aguarde a confirmação no mapa!";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Checkout(string codigo)
    {
        return View((object)codigo);
    }
}