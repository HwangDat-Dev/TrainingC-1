using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.Features.Auth.Commands.Login;
using Training.ExpenseTracker.Application.Features.Auth.Commands.Register;
using Training.ExpenseTracker.Application.Features.Auth.Query.GetUserInfo;
using Training.ExpenseTracker.Infrastructure.Security;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerHandler;
    private readonly LoginCommandHandler _loginHandler;
    private readonly GetUserInfoQueryHandler _getUserInfoHandler;

    public AuthController(
        RegisterCommandHandler registerHandler,
        LoginCommandHandler loginHandler,
        GetUserInfoQueryHandler getUserInfoHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _getUserInfoHandler = getUserInfoHandler;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request);
        var result = await _registerHandler.Handle(command, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request);
        var result = await _loginHandler.Handle(command, ct);
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
        var query = new GetUserInfoQuery(userId);
        var result = await _getUserInfoHandler.Handle(query, ct);
        return Ok(result);
    }
}