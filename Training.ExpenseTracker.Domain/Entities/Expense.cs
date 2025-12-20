

namespace Training.ExpenseTracker.Domain.Entities;

public class Expense 
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public decimal Amount { get; set; }

    public string Category { get; set; } = default!;
    public string? Note { get; set; }

    public DateTime SpendDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
}