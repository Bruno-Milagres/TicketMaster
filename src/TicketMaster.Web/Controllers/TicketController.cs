using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketMaster.Application.Services;

namespace TicketMaster.Web.Controllers;

public class TicketController : Controller
{
    private readonly TicketService _ticketService;

    public TicketController(TicketService ticketService)
    {
        _ticketService = ticketService;
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

    [HttpGet]
    public IActionResult Pagar()
    {
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Reservar(string assentoCodigo)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _ticketService.ReservarAssentoAsync(assentoCodigo, usuarioLogadoId);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction("Index");
        }

        TempData["Sucesso"] = $"Assento {assentoCodigo} reservado! Você tem 15 minutos para finalizar a compra.";
        return RedirectToAction("Checkout", new { codigo = assentoCodigo });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Pagar(string assentoCodigo)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuarioLogadoId = Guid.Parse(userIdString!);

        var resultado = await _ticketService.ConfirmarPagamentoAsync(assentoCodigo, usuarioLogadoId);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction("Index");
        }

        TempData["Sucesso"] = $"Pagamento confirmado para o assento {assentoCodigo}!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Checkout(string codigo)
    {
        return View();
    }
}