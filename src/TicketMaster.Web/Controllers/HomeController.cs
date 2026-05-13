using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Queries.ListarEventosAtivos;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _db;

    public HomeController(IMediator mediator, AppDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    public async Task<IActionResult> Index(int pagina = 1, CancellationToken cancellationToken = default)
    {
        var eventos = await _mediator.Send(new ListarEventosAtivosQuery(pagina), cancellationToken);
        return View(eventos);
    }

    public async Task<IActionResult> MeusPedidos(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Challenge();

        var pedidos = await _db.Pedidos
            .Where(p => p.UsuarioId == userId)
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataPedido)
            .Select(p => new PedidoResumoView
            {
                Id = p.Id,
                DataPedido = p.DataPedido,
                Status = p.Status.ToString(),
                Total = p.Total,
                TotalItens = p.Itens.Sum(i => i.Quantidade)
            })
            .ToListAsync(cancellationToken);

        return View(pedidos);
    }
}

public class PedidoResumoView
{
    public Guid Id { get; set; }
    public DateTime DataPedido { get; set; }
    public string Status { get; set; } = "";
    public decimal Total { get; set; }
    public int TotalItens { get; set; }
}
