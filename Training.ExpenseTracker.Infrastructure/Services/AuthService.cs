using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;
using Training.ExpenseTracker.Infrastructure.Persistence;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Training.ExpenseTracker.Infrastructure.Security;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext db, IOptions<JwtSettings> jwtOptions)
    {
        _db = db;
        _jwtSettings = jwtOptions.Value;
    }


    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var username = request.Username.Trim();
        var exists = await _db.Users.AnyAsync(x => x.Username == username, ct);

        if (exists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username đã tồn tại",
            };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            Success = true,
            Message = "Đăng ký thành công",
        };
    }



 
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var username = request.Username.Trim();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == username, ct);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Sai username hoặc mật khẩu",
            };
        }

        var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!ok)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Sai username hoặc mật khẩu",
            };
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Đăng nhập thành công",
            Token = GenerateJwt(user)
        };
    }

    public async Task<AuthInfoResponse> GetInfoAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        if (user == null)
            throw new InvalidOperationException("User không tồn tại");

        return new AuthInfoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
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