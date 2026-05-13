using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Application.Queries.ListarEventosAtivos;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "AdminGeral,Admin")]
public class EventosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public EventosController(AppDbContext db, IMediator mediator, IWebHostEnvironment env)
    {
        _db = db;
        _mediator = mediator;
        _env = env;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var eventos = await _db.Events
            .OrderByDescending(e => e.EventDate)
            .ToListAsync(ct);
        return View(eventos);
    }

    public IActionResult Create()
    {
        ViewBag.Salas = new SelectList(_db.Rooms.ToList(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event evento, Guid roomId)
    {
        evento = new Event(evento.Title, evento.EventDate, roomId);
        _db.Events.Add(evento);
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = $"Evento \"{evento.Title}\" criado!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();
        ViewBag.Salas = new SelectList(_db.Rooms.ToList(), "Id", "Name", evento.RoomId);
        return View(evento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Event model, Guid roomId, CancellationToken ct = default)
    {
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();

        typeof(Event).GetProperty(nameof(Event.Title))!.SetValue(evento, model.Title);
        typeof(Event).GetProperty(nameof(Event.EventDate))!.SetValue(evento, model.EventDate);
        typeof(Event).GetProperty(nameof(Event.RoomId))!.SetValue(evento, roomId);

        await _db.SaveChangesAsync(ct);
        TempData["Sucesso"] = "Evento atualizado!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publicar(Guid id, CancellationToken ct = default)
    {
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();
        evento.Publicar();
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct = default)
    {
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();
        evento.Cancelar();
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();
        _db.Events.Remove(evento);
        await _db.SaveChangesAsync(ct);
        TempData["Sucesso"] = "Evento excluído.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImagem(Guid eventId, IFormFile imagemEvento)
    {
        var evento = await _db.Events.FindAsync(eventId);
        if (evento == null) return NotFound();

        if (imagemEvento == null || imagemEvento.Length == 0)
        {
            TempData["Erro"] = "Selecione uma imagem.";
            return RedirectToAction(nameof(Edit), new { id = eventId });
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(imagemEvento.FileName).ToLower();
        if (!allowed.Contains(ext))
        {
            TempData["Erro"] = "Formato não permitido. Use JPG, PNG ou WebP.";
            return RedirectToAction(nameof(Edit), new { id = eventId });
        }

        var fileName = $"evento-{evento.Id}{ext}";
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "eventos");
        Directory.CreateDirectory(uploadDir);
        var path = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await imagemEvento.CopyToAsync(stream);

        evento.DefinirImagem($"/uploads/eventos/{fileName}");
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = "Imagem atualizada!";
        return RedirectToAction(nameof(Edit), new { id = eventId });
    }
}
