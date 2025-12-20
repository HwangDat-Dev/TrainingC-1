namespace Training.ExpenseTracker.Application.DTO.Expenses;

public class GetExpensesRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Category { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public ExpenseSort Sort { get; set; } = ExpenseSort.SpendDateDesc;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}