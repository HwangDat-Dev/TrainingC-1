using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.UploadImageExpense;

public sealed class AddReceiptImageToExpenseCommandHandler
    : ICommandHandler<AddReceiptImageToExpenseCommand, ResponseExpenses>
{
    private readonly IAppDbContext _db;
    private readonly IReceiptStorage _storage;

    public AddReceiptImageToExpenseCommandHandler(IAppDbContext db, IReceiptStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<ResponseExpenses> Handle(AddReceiptImageToExpenseCommand command, CancellationToken ct)
    {
        var expense = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == command.ExpenseId && x.UserId == command.UserId, ct);

        if (expense is null)
            throw new ArgumentException("Không tìm thấy chi phí");

        if (!string.IsNullOrWhiteSpace(expense.ImageUrlId))
        {
            await _storage.DeleteAsync(expense.ImageUrlId, ct);
        }

        var (imageUrl, imageUrlId) = await _storage.UploadAsync(command.Stream, command.FileName, ct);

        expense.ImageUrl = imageUrl;
        expense.ImageUrlId = imageUrlId;
        expense.UpdatedAt = DateTime.UtcNow; 

        await _db.SaveChangesAsync(ct);

        return new ResponseExpenses
        {
            Id = expense.Id,
            UserId = expense.UserId,
            Amount = expense.Amount,
            Category = expense.Category,
            Note = expense.Note,
            SpendDate = expense.SpendDate,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt,
            ImageUrl = expense.ImageUrl,
            ImageUrlId = expense.ImageUrlId
        };
    }
}