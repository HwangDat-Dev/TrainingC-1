namespace Training.ExpenseTracker.Application.DTO.Expenses;

public class ResponseExpenses
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public decimal Amount { get; set; }
    public string Category { get; set; } = default!;
    public string? Note { get; set; }

    public DateTime SpendDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}