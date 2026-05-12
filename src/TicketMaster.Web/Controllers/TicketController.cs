using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketMaster.Application.Messages;
using TicketMaster.Application.Services;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.Controllers;

public class TicketController : Controller
{
    private readonly TicketService _ticketService;
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AppDbContext _context;

    public TicketController(TicketService ticketService, IHubContext<TicketHub> hubContext, IPublishEndpoint publishEndpoint, AppDbContext context)
    {
        _ticketService = ticketService;
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _context = context;
    }

    //=====================================================
    // Redireciona para a Home 
    //=====================================================
    [HttpGet]
    public async Task<IActionResult> Index(Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index), "Home");
        }

        if (eventId == Guid.Empty)
            return RedirectToAction(nameof(Index), "Home");

        var evento = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (evento == null) return NotFound("Evento não encontrado.");
        var sala = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == evento.RoomId);
        var tickets = await _ticketService.ObterPorEventoAsync(eventId);

        ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.EventId = eventId;
        ViewBag.EventName = evento.Title;
        ViewBag.RoomLayout = sala?.Layout;

        return View(tickets);
    }

    //====================================================================================================================
    // GET e POST para Reservar, Pagar, CancelarReserva e Checkout
    // * Reservar: GET para validar o eventId e redirecionar, POST para processar a reserva
    // * Pagar: GET para validar o eventId e redirecionar, POST para processar o pagamento
    // * CancelarReserva: POST para processar o cancelamento da reserva
    // * Checkout: GET para validar o eventId e redirecionar, POST para processar o checkout (se necessário)
    //====================================================================================================================
    [HttpGet]
    public IActionResult Reservar(Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para reserva.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Reservar(string assentoCodigo, Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para reserva.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _ticketService.ReservarAssentoAsync(assentoCodigo, usuarioLogadoId, eventId);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction(nameof(Index), new { eventId });
        }

        await _hubContext.Clients.Group(eventId.ToString()).SendAsync("AtualizarAssento", assentoCodigo, "Reservado");

        return RedirectToAction(nameof(Checkout), new { codigo = assentoCodigo, eventId });
    }

    [HttpGet]
    public IActionResult Pagar(Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para pagamento.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Pagar(string assentoCodigo, Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para pagamento.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var comando = new PagamentoCommand(assentoCodigo, usuarioLogadoId, eventId);

        await _publishEndpoint.Publish(comando);

        TempData["Sucesso"] = $"Seu pedido de pagamento para o assento {assentoCodigo} foi para a fila. Aguarde a confirmação no mapa!";

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpGet]
    public IActionResult Checkout(string codigo, Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para checkout.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        ViewBag.EventId = eventId;
        return View((object)codigo);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CancelarReserva(string assentoCodigo, Guid eventId)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para cancelamento.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _ticketService.CancelarReservaAsync(assentoCodigo, usuarioLogadoId, eventId);

        if (resultado.IsSuccess)
        {
            await _hubContext.Clients.Group(eventId.ToString()).SendAsync("AtualizarAssento", assentoCodigo, "Disponivel");
            TempData["Sucesso"] = "Sua reserva foi cancelada e o assento está livre novamente.";
        }
        else
        {
            TempData["Erro"] = resultado.ErrorMessage;
        }

        return RedirectToAction("Index", new { eventId });
    }
}