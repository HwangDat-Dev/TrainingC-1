namespace Training.ExpenseTracker.Application.DTO.Expenses;

public class ExpenseSummaryResponse
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }

    public List<ExpenseSummaryByCategory> ByCategory { get; set; } = new();
}

public class ExpenseSummaryByCategory
{
    public string Category { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}