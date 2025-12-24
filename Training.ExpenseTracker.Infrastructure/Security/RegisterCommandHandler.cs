using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.Features.Auth.Commands.Register;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Infrastructure.Security;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly IWriteDbContext _db;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IWriteDbContext db, ILogger<RegisterCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken ct)
    {
        _logger.LogInformation("[DB:WRITE] Register user: {Username}", command.Request.Username);
        
        var username = command.Request.Username.Trim();
        var exists = await _db.Users.AnyAsync(x => x.Username == username, ct);

        if (exists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username đã tồn tại"
            };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            Success = true,
            Message = "Đăng ký thành công"
        };
    }
}
