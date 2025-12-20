using System.ComponentModel.DataAnnotations;

namespace Training.ExpenseTracker.Application.DTO.Expenses;

public class CreateExpenses
{
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = default!;

    [MaxLength(255)]
    public string? Note { get; set; }

    public DateTime SpendDate { get; set; }
}