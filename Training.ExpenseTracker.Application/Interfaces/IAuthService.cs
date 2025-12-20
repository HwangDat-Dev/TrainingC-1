using Training.ExpenseTracker.Application.DTO;

namespace Training.ExpenseTracker.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthInfoResponse> GetInfoAsync(Guid userId, CancellationToken ct = default);
}