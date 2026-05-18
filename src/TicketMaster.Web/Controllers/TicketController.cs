using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TicketMaster.Application.Commands.CancelarReserva;
using TicketMaster.Application.Commands.ReservarAssento;
using TicketMaster.Application.Messages;
using TicketMaster.Application.Queries.ObterIngressosPorEvento;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Web.Services;

namespace TicketMaster.Web.Controllers;

public class TicketController : Controller
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public TicketController(IMediator mediator, IPublishEndpoint publishEndpoint, AppDbContext context, IConfiguration config, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _context = context;
        _config = config;
        _env = env;
    }

    //=====================================================
    // Redireciona para a Home 
    //=====================================================
    [HttpGet]
    public async Task<IActionResult> Index(Guid eventId, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index), "Home");
        }

        if (eventId == Guid.Empty)
            return RedirectToAction(nameof(Index), "Home");

        var evento = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        if (evento == null) return NotFound("Evento não encontrado.");
        var sala = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == evento.RoomId, cancellationToken);
        var tickets = await _mediator.Send(new ObterIngressosPorEventoQuery(eventId), cancellationToken);

        var sectorPrices = await _context.EventSectorPrices
            .Where(p => p.EventId == eventId)
            .ToListAsync(cancellationToken);

        var svgPath = Path.Combine(_env.WebRootPath, "svg", "theater-layout.svg");
        var svgContent = System.IO.File.Exists(svgPath)
            ? await System.IO.File.ReadAllTextAsync(svgPath, cancellationToken)
            : string.Empty;

        ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.EventId       = eventId;
        ViewBag.EventName     = evento.Title;
        ViewBag.SectorPrices  = sectorPrices;
        ViewBag.TheaterSvg    = svgContent;

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
    public async Task<IActionResult> Reservar(string assentoCodigo, Guid eventId, int category = 0, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para reserva.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _mediator.Send(
            new ReservarAssentoCommand(assentoCodigo, usuarioLogadoId, eventId),
            cancellationToken);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction(nameof(Index), new { eventId });
        }

        // Notificação SignalR enviada pelo AssentoReservadoEventHandler
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
    public async Task<IActionResult> Pagar(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para pagamento.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var comando = new PagamentoCommand(assentoCodigo, usuarioLogadoId, eventId);

        await _publishEndpoint.Publish(comando, cancellationToken);

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
    public async Task<IActionResult> CancelarReserva(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos para cancelamento.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _mediator.Send(
            new CancelarReservaCommand(assentoCodigo, usuarioLogadoId, eventId),
            cancellationToken);

        if (resultado.IsSuccess)
        {
            // Notificação SignalR enviada pelo ReservaCanceladaEventHandler
            TempData["Sucesso"] = "Sua reserva foi cancelada e o assento está livre novamente.";
        }
        else
        {
            TempData["Erro"] = resultado.ErrorMessage;
        }

        return RedirectToAction("Index", new { eventId });
    }

    //====================================================================================================================
    // Multi-reserva (JS: reserva vários assentos de uma vez)
    //====================================================================================================================
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ReservarMultiplos(string assentosCodigos, Guid eventId, int category = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assentosCodigos))
        {
            TempData["Erro"] = "Nenhum assento selecionado.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);
        var assentos = assentosCodigos.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var ultimoErro = "";

        foreach (var codigo in assentos)
        {
            var cmd = new ReservarAssentoCommand(codigo.Trim(), usuarioLogadoId, eventId);
            var resultado = await _mediator.Send(cmd, cancellationToken);
            if (!resultado.IsSuccess) ultimoErro = resultado.ErrorMessage;
        }

        if (!string.IsNullOrEmpty(ultimoErro))
            TempData["Erro"] = ultimoErro;
        else
            TempData["Sucesso"] = $"{assentos.Length} assento(s) reservado(s)!";

        return RedirectToAction(nameof(Checkout), new { codigo = assentos.Length > 0 ? assentos[0] : "", eventId });
    }

    //====================================================================================================================
    // Página de Pagamento (mostra reservas + opções de pagamento)
    //====================================================================================================================
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> PagarMultiplos(Guid eventId, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Challenge();

        var usuarioLogadoId = Guid.Parse(userIdString);
        var minhasReservas = await _context.Tickets
            .Where(t => t.EventId == eventId && t.UsuarioId == usuarioLogadoId && t.Status == TicketStatus.Reservado)
            .ToListAsync(cancellationToken);

        var evento = await _context.Events.FindAsync(new object[] { eventId }, cancellationToken);

        ViewBag.EventId = eventId;
        ViewBag.Reservas = minhasReservas;
        ViewBag.EventName = evento?.Title ?? "Evento";

        // Gera QR Code PIX mock para cada reserva
        var qrService = HttpContext.RequestServices.GetRequiredService<QrCodeService>();
        var qrCodes = new List<object>();
        foreach (var r in minhasReservas)
        {
            var payload = qrService.GerarPayloadJwt(r.Id, r.EventId, r.AssentoCodigo, userIdString);
            var qrBytes = qrService.GerarQrCodePng(payload);
            qrCodes.Add(new { seat = r.AssentoCodigo, qrBase64 = Convert.ToBase64String(qrBytes) });
        }
        ViewBag.QrCodes = qrCodes;

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ConfirmarPagamento(Guid eventId, string metodo, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return Challenge();

        var usuarioLogadoId = Guid.Parse(userIdString);
        var minhasReservas = await _context.Tickets
            .Where(t => t.EventId == eventId && t.UsuarioId == usuarioLogadoId && t.Status == TicketStatus.Reservado)
            .ToListAsync(cancellationToken);

        if (!minhasReservas.Any())
        {
            TempData["Erro"] = "Nenhuma reserva ativa.";
            return RedirectToAction("Index", new { eventId });
        }

        foreach (var ticket in minhasReservas)
        {
            var comando = new PagamentoCommand(ticket.AssentoCodigo, usuarioLogadoId, eventId);
            await _publishEndpoint.Publish(comando, cancellationToken);
        }

        TempData["Sucesso"] = $"{minhasReservas.Count} pedido(s) de pagamento enviado(s)!";
        return RedirectToAction("Index", "Home");
    }

    //====================================================================================================================
    // B4 — Página do ingresso com QR Code
    //====================================================================================================================
    [Authorize]
    public async Task<IActionResult> Ingresso(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        if (ticket == null) return NotFound();

        var evento = await _context.Events.FindAsync(new object[] { ticket.EventId }, cancellationToken);
        var qrService = HttpContext.RequestServices.GetRequiredService<QrCodeService>();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var payload = qrService.GerarPayloadJwt(ticketId, ticket.EventId, ticket.AssentoCodigo, userId);
        var qrBytes = qrService.GerarQrCodePng(payload);

        ViewBag.QrCodeBase64 = Convert.ToBase64String(qrBytes);
        ViewBag.EventName = evento?.Title ?? "Evento";
        ViewBag.SeatCode = ticket.AssentoCodigo;
        ViewBag.TicketId = ticketId;

        return View();
    }

    //====================================================================================================================
    // B4 — Endpoint de validação de QR Code (para scanner na entrada)
    //====================================================================================================================
    [HttpPost("api/tickets/validate")]
    public async Task<IActionResult> ValidateQr([FromBody] ValidateQrRequest request)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var secret = _config["Jwt:Secret"] ?? "ChaveSuperSecretaTicketMaster2026!";
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(secret));

            var result = await handler.ValidateTokenAsync(request.QrPayload, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "ticketmaster",
                ValidateAudience = true,
                ValidAudience = "ticket-validator",
                ValidateLifetime = true,
                IssuerSigningKey = key
            });

            if (!result.IsValid)
                return Forbid();

            var tid = Guid.Parse(result.Claims["tid"].ToString()!);
            var ticket = await _context.Tickets.FindAsync(tid);
            if (ticket == null || ticket.Status != TicketStatus.Vendido)
                return StatusCode(403, new { error = "TICKET_INVALIDO" });

            return Ok(new { status = "ACCESS_GRANTED", seatCode = ticket.AssentoCodigo });
        }
        catch
        {
            return StatusCode(403, new { error = "INVALID_TOKEN" });
        }
    }
}

public record ValidateQrRequest(string QrPayload);
