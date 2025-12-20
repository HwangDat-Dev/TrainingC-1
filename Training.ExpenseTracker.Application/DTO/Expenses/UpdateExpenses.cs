using System.ComponentModel.DataAnnotations;

namespace Training.ExpenseTracker.Application.DTO.Expenses;

public class UpdateExpenses
{
    public decimal? Amount { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }

    public DateTime? SpendDate { get; set; }
}