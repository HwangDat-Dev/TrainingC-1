namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid UserId, Guid ExpenseId);
