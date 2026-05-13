using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;
using TicketMaster.Web.Models;

namespace TicketMaster.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "AdminGeral,Admin")]
public class EventosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public EventosController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private async Task<Guid> GetSalaFixaIdAsync()
    {
        var sala = await _db.Rooms.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Sala do teatro não encontrada. Execute o seed primeiro.");
        return sala.Id;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var eventos = await _db.Events
            .OrderByDescending(e => e.EventDate)
            .ToListAsync(ct);
        return View(eventos);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventoFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var roomId = await GetSalaFixaIdAsync();
        var evento = new Event(model.Title, model.EventDate, roomId);
        _db.Events.Add(evento);
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = $"Evento \"{evento.Title}\" criado!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();
        ViewBag.EventoId = id;
        ViewBag.ImagemUrl = evento.ImagemUrl;
        var vm = new EventoFormViewModel
        {
            Title = evento.Title,
            EventDate = evento.EventDate,
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EventoFormViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventoId = id;
            return View(model);
        }
        var evento = await _db.Events.FindAsync(new object[] { id }, ct);
        if (evento == null) return NotFound();

        typeof(Event).GetProperty(nameof(Event.Title))!.SetValue(evento, model.Title);
        typeof(Event).GetProperty(nameof(Event.EventDate))!.SetValue(evento, model.EventDate);

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
        var ext = Path.GetExtension(imagemEvento.FileName).ToLowerInvariant();
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
