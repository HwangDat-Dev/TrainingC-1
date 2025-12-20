using System.Text.Json.Serialization;

namespace Training.ExpenseTracker.Application.DTO;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Token { get; set; }
}