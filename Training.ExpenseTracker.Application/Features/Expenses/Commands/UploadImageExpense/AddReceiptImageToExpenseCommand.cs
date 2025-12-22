namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.UploadImageExpense;

public sealed record AddReceiptImageToExpenseCommand(Guid UserId, Guid ExpenseId, Stream Stream, string FileName );