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

    [HttpGet]
    public async Task<IActionResult> Index(Guid eventId, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid || eventId == Guid.Empty)
            return RedirectToAction(nameof(Index), "Home");

        var evento = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        if (evento == null) return NotFound("Evento não encontrado.");

        var tickets = await _mediator.Send(new ObterIngressosPorEventoQuery(eventId), cancellationToken);

        var sectorPrices = await _context.EventSectorPrices
            .Where(p => p.EventId == eventId)
            .ToListAsync(cancellationToken);

        var svgPath = Path.Combine(_env.WebRootPath, "svg", "theater-layout.svg");
        var svgContent = System.IO.File.Exists(svgPath)
            ? await System.IO.File.ReadAllTextAsync(svgPath, cancellationToken)
            : string.Empty;

        ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.EventId = eventId;
        ViewBag.EventName = evento.Title;
        ViewBag.SectorPrices = sectorPrices;
        ViewBag.TheaterSvg = svgContent;

        return View(tickets);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Reservar(string assentoCodigo, Guid eventId, int category = 0, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _mediator.Send(
            new ReservarAssentoCommand(assentoCodigo, usuarioLogadoId, eventId), cancellationToken);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction(nameof(Index), new { eventId });
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Pagar(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var comando = new PagamentoCommand(assentoCodigo, usuarioLogadoId, eventId);
        await _publishEndpoint.Publish(comando, cancellationToken);

        TempData["Sucesso"] = $"Pedido de pagamento enviado para {assentoCodigo}!";
        return RedirectToAction(nameof(Index), new { eventId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CancelarReserva(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _mediator.Send(
            new CancelarReservaCommand(assentoCodigo, usuarioLogadoId, eventId), cancellationToken);

        if (resultado.IsSuccess)
            TempData["Sucesso"] = "Reserva cancelada.";
        else
            TempData["Erro"] = resultado.ErrorMessage;

        var returnUrl = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("/Ticket/PagarMultiplos"))
            return RedirectToAction(nameof(PagarMultiplos), new { eventId });

        return RedirectToAction("Index", new { eventId });
    }

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

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> PagarMultiplos(Guid eventId, CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return Challenge();

        var usuarioLogadoId = Guid.Parse(userIdString);
        var minhasReservas = await _context.Tickets
            .Where(t => t.EventId == eventId && t.UsuarioId == usuarioLogadoId && t.Status == TicketStatus.Reservado)
            .ToListAsync(cancellationToken);

        var evento = await _context.Events.FindAsync(new object[] { eventId }, cancellationToken);

        ViewBag.EventId = eventId;
        ViewBag.Reservas = minhasReservas;
        ViewBag.EventName = evento?.Title ?? "Evento";
        ViewBag.QrCodeBase64 = "";

        var sectorPrices = await _context.EventSectorPrices
            .Where(p => p.EventId == eventId)
            .ToListAsync(cancellationToken);
        ViewBag.SectorPrices = sectorPrices;

        if (minhasReservas.Any())
        {
            var qrService = HttpContext.RequestServices.GetRequiredService<QrCodeService>();
            var codigosAssentos = string.Join(",", minhasReservas.Select(r => r.AssentoCodigo));
            var pixPayload = qrService.GerarPayloadJwt(minhasReservas.First().Id, eventId, codigosAssentos, userIdString);
            var qrBytes = qrService.GerarQrCodePng(pixPayload);
            ViewBag.QrCodeBase64 = Convert.ToBase64String(qrBytes);

            var total = minhasReservas.Sum(r =>
            {
                var sector = GetSectorFromCode(r.AssentoCodigo);
                var price = sectorPrices.FirstOrDefault(p => p.Sector == sector && p.Category == TicketMaster.Domain.Enums.TicketCategory.Inteira);
                return price?.Price ?? 0;
            });
            ViewBag.Total = total;
        }
        else
        {
            ViewBag.Total = 0m;
        }

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

        TempData["Sucesso"] = $"{minhasReservas.Count} pedido(s) enviado(s)!";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Ingresso(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
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

    [HttpPost("api/tickets/validate")]
    public async Task<IActionResult> ValidateQr([FromBody] ValidateQrRequest request)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var secret = _config["Jwt:Secret"] ?? "ChaveSuperSecretaTicketMaster2026!";
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));

            var result = await handler.ValidateTokenAsync(request.QrPayload, new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = "ticketmaster",
                ValidateAudience = true, ValidAudience = "ticket-validator",
                ValidateLifetime = true, IssuerSigningKey = key
            });

            if (!result.IsValid) return Forbid();

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

    private static string GetSectorFromCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return "PlateiaCentro";
        if (code.StartsWith("FRE-") || code.StartsWith("FRD-")) return "Frisa";
        if (code.StartsWith("CAME-") || code.StartsWith("CAMD-")) return "Camarote";
        if (code.StartsWith("BAL-")) return "Balcao";
        if (code.StartsWith("AC-")) return "Acessibilidade";
        var c = code[0];
        if (c >= 'A' && c <= 'E') return "PlateiaFrente";
        if (c >= 'F' && c <= 'P') return "PlateiaCentro";
        if (c >= 'Q' && c <= 'V') return "PlateiaFundo";
        return "PlateiaCentro";
    }
}

public record ValidateQrRequest(string QrPayload);
