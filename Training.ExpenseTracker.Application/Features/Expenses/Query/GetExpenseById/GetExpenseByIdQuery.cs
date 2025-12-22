namespace Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseById;

public sealed record GetExpenseByIdQuery(Guid UserId, Guid ExpenseId);
