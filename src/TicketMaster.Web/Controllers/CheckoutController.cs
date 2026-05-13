using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TicketMaster.Application.Commands.ReservarAssento;
using TicketMaster.Application.Interfaces;
using TicketMaster.Domain.Entities;
using TicketMaster.Application.Notifications;
using TicketMaster.Domain.Events;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;
    private readonly ITicketRepository _ticketRepo;

    public CheckoutController(AppDbContext db, IMediator mediator, ITicketRepository ticketRepo)
    {
        _db = db;
        _mediator = mediator;
        _ticketRepo = ticketRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pedido(string clienteNome, string clienteEmail, string itensJson)
    {
        List<CarrinhoItem>? itens;
        try { itens = JsonSerializer.Deserialize<List<CarrinhoItem>>(itensJson); }
        catch { itens = null; }
        if (itens == null || itens.Count == 0)
            return Content("sem itens");

        var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var pedido = new Pedido(usuarioId, clienteNome, clienteEmail);

        foreach (var i in itens)
        {
            // Se tem assento específico, reserva via command existente
            if (!string.IsNullOrWhiteSpace(i.SeatId))
            {
                var eventId = Guid.Parse(i.EventId);
                var result = await _mediator.Send(new ReservarAssentoCommand(i.SeatId, Guid.NewGuid(), eventId));
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View("Index");
                }
            }

            var tipo = await _db.TiposIngresso.FirstOrDefaultAsync(t => t.Id == Guid.Parse(i.TipoIngressoId));
            if (tipo == null || tipo.QuantidadeDisponivel < i.Quantidade)
                return Content("sem estoque");
            tipo.ReservarEstoque(i.Quantidade);
            pedido.AdicionarItem(tipo.Id, tipo.Nome, i.Quantidade, tipo.Preco);
        }
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        await _mediator.Publish(new PedidoConfirmadoNotification(new PedidoConfirmadoEvent(pedido, clienteEmail, clienteNome)));

        return RedirectToAction("Sucesso");
    }

    public IActionResult Sucesso() => View();

    public class CarrinhoItem
    {
        public string TipoIngressoId { get; set; } = "";
        public string EventId { get; set; } = "";
        public string NomeEvento { get; set; } = "";
        public string NomeIngresso { get; set; } = "";
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
        public string? SeatId { get; set; }
    }
}
