using System.Text.Json.Serialization;

namespace Training.ExpenseTracker.Application.DTO;

public class LoginRequest
{
    public string Username { get; set; } = default;
    public string Password { get; set; } = default;
}