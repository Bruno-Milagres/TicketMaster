using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EventosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public EventosController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
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
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(imagemEvento.FileName).ToLower();
        if (!allowed.Contains(ext))
        {
            TempData["Erro"] = "Formato não permitido. Use JPG, PNG ou WebP.";
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        var fileName = $"evento-{evento.Id}{ext}";
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "eventos");
        Directory.CreateDirectory(uploadDir);
        var path = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await imagemEvento.CopyToAsync(stream);

        evento.DefinirImagem($"/uploads/eventos/{fileName}");
        await _db.SaveChangesAsync();

        TempData["Sucesso"] = "Imagem atualizada com sucesso!";
        return RedirectToAction("Index", "Home", new { area = "" });
    }
}
