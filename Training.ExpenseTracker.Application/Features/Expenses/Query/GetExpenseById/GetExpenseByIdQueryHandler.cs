using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseById;

public sealed class GetExpenseByIdQueryHandler
    : IQueryHandler<GetExpenseByIdQuery, ResponseExpenses>
{
    private readonly IAppDbContext _db;

    public GetExpenseByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseExpenses> Handle(GetExpenseByIdQuery query, CancellationToken ct)
    {
        var entity = await _db.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.ExpenseId && x.UserId == query.UserId, ct);

        if (entity == null) throw new ArgumentException("Chi phí không tồn tại");

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