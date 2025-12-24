using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.Features.Auth.Commands.Login;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Training.ExpenseTracker.Infrastructure.Security;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IReadDbContext _db;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IReadDbContext db, IOptions<JwtSettings> jwtOptions, ILogger<LoginCommandHandler> logger)
    {
        _db = db;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        _logger.LogInformation("[DB:READ] Login user: {Username}", command.Request.Username);
        
        var username = command.Request.Username.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == username, ct);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Sai username hoặc mật khẩu"
            };
        }

        var ok = BCrypt.Net.BCrypt.Verify(command.Request.Password, user.PasswordHash);
        if (!ok)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Sai username hoặc mật khẩu"
            };
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Đăng nhập thành công",
            Token = GenerateJwt(user)
        };
    }

    private string GenerateJwt(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
            throw new InvalidOperationException("JWT Secret chưa được cấu hình");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
