using System.ComponentModel.DataAnnotations;

namespace Training.ExpenseTracker.Application.DTO.Expenses;

public class UpdateImageExpense
{
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = default!;

    [MaxLength(255)]
    public string ImageUrlId { get; set; } = default!;
}