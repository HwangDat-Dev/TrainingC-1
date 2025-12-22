using Training.ExpenseTracker.Application.DTO.Expenses;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
    Guid UserId,
    Guid ExpenseId,
    UpdateExpenses Request
);
