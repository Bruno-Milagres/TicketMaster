using Microsoft.AspNetCore.Mvc;
using TicketMaster.Application.Interfaces;
using TicketMaster.Application.Services;

namespace TicketMaster.Web.Controllers;

public class TicketController : Controller
{
    private readonly TicketService _ticketService;
    private readonly ITicketRepository _ticketRepository;

    public TicketController(TicketService ticketService, ITicketRepository ticketRepository)
    {
        _ticketService = ticketService;
        _ticketRepository = ticketRepository;
    }

    // GET: /Ticket/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Pega todos os ingressos do banco (A1, A2, A3...)
        var tickets = await _ticketRepository.ObterTodosAsync();

        // Manda para a tela
        return View(tickets);
    }

    // POST: /Ticket/Reservar
    [HttpPost]
    public async Task<IActionResult> Reservar(string assentoCodigo)
    {
        // Simulando o ID do usuario logado (No futuro pegamos da Autenticação do .NET)
        var usuarioLogadoId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var resultado = await _ticketService.ReservarAssentoAsync(assentoCodigo, usuarioLogadoId);

        if (!resultado.IsSuccess)
        {
            // O ingresso ja foi pego ou estourou concorrência!
            TempData["Erro"] = resultado.ErrorMessage;
            return RedirectToAction("Index");
        }

        // Sucesso! Vai para a tela de pagamento (Checkout)
        TempData["Sucesso"] = $"Assento {assentoCodigo} reservado! Você tem 15 minutos para finalizar a compra.";
        return RedirectToAction("Checkout", new { codigo = assentoCodigo });
    }

    [HttpGet]
    public IActionResult Checkout(string codigo)
    {
        // Tela onde o usuario digita o cartao de credito
        return View();
    }
}