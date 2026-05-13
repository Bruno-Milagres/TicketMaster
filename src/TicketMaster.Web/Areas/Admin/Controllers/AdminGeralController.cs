using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        var adminGerals = await _userManager.GetUsersInRoleAsync("AdminGeral");
        var admins = await _userManager.GetUsersInRoleAsync("Admin");

        var adminUsers = adminGerals
            .Concat(admins)
            .DistinctBy(u => u.Id)
            .Select(u => new AdminUserView
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Roles = string.Join(", ", GetRolesForUser(u, adminGerals, admins))
            })
            .OrderBy(u => u.Email)
            .ToList();

        return View(adminUsers);
    }

    private static List<string> GetRolesForUser(IdentityUser user, IList<IdentityUser> adminGerals, IList<IdentityUser> admins)
    {
        var roles = new List<string>();
        if (adminGerals.Any(a => a.Id == user.Id)) roles.Add("AdminGeral");
        if (admins.Any(a => a.Id == user.Id)) roles.Add("Admin");
        return roles;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

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
