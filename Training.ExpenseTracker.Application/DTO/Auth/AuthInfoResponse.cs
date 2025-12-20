namespace Training.ExpenseTracker.Application.DTO;

public class AuthInfoResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Role { get; set; } = default!;
}