namespace Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseSummary;

public sealed record GetExpenseSummaryQuery(Guid UserId, DateTime? From, DateTime? To);
