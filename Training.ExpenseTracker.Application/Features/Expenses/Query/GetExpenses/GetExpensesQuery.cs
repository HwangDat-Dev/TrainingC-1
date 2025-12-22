using Training.ExpenseTracker.Application.DTO.Expenses;

namespace Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenses;

public sealed record GetExpensesQuery(Guid UserId, GetExpensesRequest Request);

