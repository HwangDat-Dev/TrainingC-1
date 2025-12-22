using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseCommandHandler
    : ICommandHandler<CreateExpenseCommand, ResponseExpenses>
{
    private readonly IAppDbContext _db;

    public CreateExpenseCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseExpenses> Handle(CreateExpenseCommand command, CancellationToken ct)
    {
        var req = command.Request;

        if (string.IsNullOrWhiteSpace(req.Category))
            throw new ArgumentException("Doanh mục bắt buộc phải có");

        if (req.Amount <= 0)
            throw new ArgumentException("Số tiền phải lớn hơn 0");

        var userExists = await _db.Users.AnyAsync(x => x.Id == command.UserId, ct);
        if (!userExists) throw new ArgumentException("User không tồn tại");

        var entity = new Expense()
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Category = req.Category.Trim(),
            Amount = req.Amount,
            Note = req.Note,
            SpendDate = req.SpendDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ImageUrl = null,
            ImageUrlId = null
        };

        _db.Expenses.Add(entity);
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