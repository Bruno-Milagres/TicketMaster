using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    public async Task<IActionResult> Reservar(string assentoCodigo)
    {
        // TODO: substituir pelo ID do usuário autenticado via ASP.NET Identity
        var usuarioLogadoId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var resultado = await _ticketService.ReservarAssentoAsync(assentoCodigo, usuarioLogadoId);

        if (!resultado.IsSuccess)
        {
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction("Index");
        }

        TempData["Sucesso"] = $"Assento {assentoCodigo} reservado! Você tem 15 minutos para finalizar a compra.";
        return RedirectToAction("Checkout", new { codigo = assentoCodigo });
    }

    [HttpGet]
    public IActionResult Checkout(string codigo)
    {
        return View();
    }
}