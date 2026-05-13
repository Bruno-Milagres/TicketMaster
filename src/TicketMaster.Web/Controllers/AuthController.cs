using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketMaster.Web.Services;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _db;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        TokenService tokenService,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { error = "Credenciais inválidas." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { error = "Credenciais inválidas." });

        var accessToken = _tokenService.GerarAccessToken(user.Id, user.Email!);
        var refreshToken = _tokenService.GerarRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(7)));
        await _db.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken,
            expiresIn = 900 // 15 min em segundos
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.IsActive);

        if (stored == null)
            return Unauthorized(new { error = "Refresh token inválido ou expirado." });

        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user == null)
            return Unauthorized(new { error = "Usuário não encontrado." });

        stored.Revoke();
        var newAccessToken = _tokenService.GerarAccessToken(user.Id, user.Email!);
        var newRefreshToken = _tokenService.GerarRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7)));
        await _db.SaveChangesAsync();

        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken, expiresIn = 900 });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequest request)
    {
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.IsActive);

        if (stored != null)
        {
            stored.Revoke();
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
