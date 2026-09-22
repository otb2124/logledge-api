using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using logledge_api.DTOs;
using logledge_api.Models;
using logledge_api.Services;

namespace logledge_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private const string CookieName = "logledge_token";

    public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Conflict(new { message = "A user with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        SetTokenCookie(user);

        return Ok(new { userId = user.Id, email = user.Email, displayName = user.DisplayName });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Unauthorized(new { message = "Invalid email or password." });

        SetTokenCookie(user);

        return Ok(new { userId = user.Id, email = user.Email, displayName = user.DisplayName });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });
        return Ok(new { message = "Logged out." });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return NotFound();

        return Ok(new { userId = user.Id, email = user.Email, displayName = user.DisplayName });
    }

    private void SetTokenCookie(ApplicationUser user)
    {
        var (token, expiresAt) = _tokenService.CreateToken(user);

        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,       // JS can never read this — mitigates XSS token theft
            Secure = true,         // only sent over HTTPS
            SameSite = SameSiteMode.None, // needed for cross-port localhost (4200 -> 7040); use Lax/Strict once same-site in prod
            Expires = expiresAt,
            Path = "/"
        });
    }
}