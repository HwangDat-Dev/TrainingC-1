using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseCommandHandler
    : ICommandHandler<UpdateExpenseCommand, ResponseExpenses>
{
    private readonly IWriteDbContext _db;

    public UpdateExpenseCommandHandler(IWriteDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseExpenses> Handle(UpdateExpenseCommand command, CancellationToken ct)
    {
        var req = command.Request;

        if (req.Category == null)
            throw new ArgumentException("Doanh mục bắt buộc phải có");

        if (req.Amount <= 0)
            throw new ArgumentException("Số tiền phải lớn hơn 0");

        var entity = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == command.ExpenseId && x.UserId == command.UserId, ct);

        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy chi phí !!");

        req = command.Request;

        if (req.Amount.HasValue)
        {
            if (req.Amount.Value <= 0)
                throw new ArgumentException("Số tiền phải lớn hơn 0");
            entity.Amount = req.Amount.Value;
        }

        if (req.SpendDate.HasValue)
        {
            entity.SpendDate = req.SpendDate.Value;
        }

        if (req.Category != null) 
        {
            if (string.IsNullOrWhiteSpace(req.Category))
                throw new ArgumentException("Doanh mục không được rỗng");
            entity.Category = req.Category.Trim();
        }

        if (req.Note != null) 
        {
            entity.Note = req.Note;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new ResponseExpenses
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Amount = entity.Amount,
            Category = entity.Category,
            Note = entity.Note,
            SpendDate = entity.SpendDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ImageUrl = entity.ImageUrl,
            ImageUrlId = entity.ImageUrlId
        };
    }
}