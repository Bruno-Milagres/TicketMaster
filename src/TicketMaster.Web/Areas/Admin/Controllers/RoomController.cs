using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketMaster.Application.Commands.AtualizarSala;
using TicketMaster.Application.Commands.CriarSala;
using TicketMaster.Application.Commands.ExcluirSala;
using TicketMaster.Application.Queries.ListarSalas;
using TicketMaster.Application.Queries.ObterSalaPorId;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoomController : Controller
{
    private readonly IMediator _mediator;

    public RoomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var salas = await _mediator.Send(new ListarSalasQuery(), cancellationToken);
        return View(salas);
    }

    public IActionResult Create()
    {
        return View(new CriarSalaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarSalaViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var layout = new Room.RoomLayout
        {
            MaxColumns = model.MaxColumns,
            MaxRows = model.MaxRows,
            Seats = model.ObterAssentos()
        };

        var salaId = await _mediator.Send(
            new CriarSalaCommand(model.Nome, layout), cancellationToken);

        TempData["Sucesso"] = $"Sala \"{model.Nome}\" criada com sucesso!";
        return RedirectToAction(nameof(Edit), new { id = salaId });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        var sala = await _mediator.Send(new ObterSalaPorIdQuery(id), cancellationToken);
        if (sala == null) return NotFound();

        var model = new CriarSalaViewModel
        {
            Nome = sala.Name,
            MaxColumns = sala.Layout.MaxColumns,
            MaxRows = sala.Layout.MaxRows,
            AssentosJson = System.Text.Json.JsonSerializer.Serialize(sala.Layout.Seats)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CriarSalaViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var layout = new Room.RoomLayout
        {
            MaxColumns = model.MaxColumns,
            MaxRows = model.MaxRows,
            Seats = model.ObterAssentos()
        };

        await _mediator.Send(new AtualizarSalaCommand(id, model.Nome, layout), cancellationToken);

        TempData["Sucesso"] = "Sala atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var resultado = await _mediator.Send(new ExcluirSalaCommand(id), cancellationToken);

        if (!resultado.IsSuccess)
            TempData["Erro"] = resultado.ErrorMessage;
        else
            TempData["Sucesso"] = "Sala excluída com sucesso.";

        return RedirectToAction(nameof(Index));
    }
}

public class CriarSalaViewModel
{
    public string Nome { get; set; } = string.Empty;
    public int MaxColumns { get; set; } = 5;
    public int MaxRows { get; set; } = 5;
    public string? AssentosJson { get; set; }

    public List<Room.SeatCoordinate> ObterAssentos()
    {
        if (string.IsNullOrWhiteSpace(AssentosJson))
            return new();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Room.SeatCoordinate>>(AssentosJson)
                   ?? new();
        }
        catch
        {
            return new();
        }
    }
}
