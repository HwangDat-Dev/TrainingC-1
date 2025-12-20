using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.Interfaces;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("info")]
    public async Task<IActionResult> Info(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        

        if (userIdClaim == null)
            return Unauthorized(new { message = "Token không hợp lệ" });
        
        var userId = Guid.Parse(userIdClaim.Value);
        var result = await _auth.GetInfoAsync(userId, ct);
        return Ok(result);
    }
}