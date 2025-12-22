using Training.ExpenseTracker.Application.DTO.Expenses;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(Guid UserId, CreateExpenses Request);
