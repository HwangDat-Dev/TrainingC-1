using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Auth.Query.GetUserInfo;

public sealed class GetUserInfoQueryHandler : IQueryHandler<GetUserInfoQuery, AuthInfoResponse>
{
    private readonly IReadDbContext _db;
    private readonly ILogger<GetUserInfoQueryHandler> _logger;

    public GetUserInfoQueryHandler(IReadDbContext db, ILogger<GetUserInfoQueryHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AuthInfoResponse> Handle(GetUserInfoQuery query, CancellationToken ct)
    {
        _logger.LogInformation("[DB:READ] GetUserInfo: {UserId}", query.UserId);
        
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == query.UserId, ct);

        if (user == null)
            throw new InvalidOperationException("User không tồn tại");

        return new AuthInfoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
}
