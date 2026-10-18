using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CreditWorks.Api.Models;
using CreditWorks.Application.Interfaces;

namespace CreditWorks.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    public const string LoginRateLimitPolicy = "login";

    private readonly IAdminAuthService _authService;

    public AuthController(IAdminAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting(LoginRateLimitPolicy)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (!await _authService.ValidateCredentialsAsync(
                request.Username, request.Password))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid username or password.");
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, request.Username.Trim())],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        return Ok(new { username = request.Username.Trim() });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    // The frontend calls this on load to find out whether
    // the auth cookie is still valid.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new { username = User.Identity!.Name });
    }
}
