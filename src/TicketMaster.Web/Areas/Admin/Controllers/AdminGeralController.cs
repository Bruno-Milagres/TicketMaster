using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TicketMaster.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "AdminGeral")]
public class AdminGeralController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminGeralController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var adminRoleIds = new[] { "AdminGeral", "Admin" }
            .Select(r => _roleManager.FindByNameAsync(r).Result?.Id)
            .Where(id => id != null)
            .ToList();

        var adminUsers = await _userManager.Users
            .Where(u => _userManager.GetRolesAsync(u).Result.Any(r => r == "AdminGeral" || r == "Admin"))
            .Select(u => new AdminUserView
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Roles = string.Join(", ", _userManager.GetRolesAsync(u).Result)
            })
            .ToListAsync();

        return View(adminUsers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Não permite remover AdminGeral do último AdminGeral
        if (role == "AdminGeral")
        {
            var adminGerals = await _userManager.GetUsersInRoleAsync("AdminGeral");
            if (adminGerals.Count <= 1 && adminGerals.Any(u => u.Id == userId))
            {
                TempData["Erro"] = "Não é possível remover o último AdminGeral.";
                return RedirectToAction(nameof(Index));
            }
        }

        await _userManager.RemoveFromRoleAsync(user, role);
        TempData["Sucesso"] = $"Role '{role}' removida de {user.Email}.";
        return RedirectToAction(nameof(Index));
    }
}

public class AdminUserView
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string Roles { get; set; } = "";
}
