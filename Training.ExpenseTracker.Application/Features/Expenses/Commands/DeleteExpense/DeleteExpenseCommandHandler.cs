using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandHandler
    : ICommandHandler<DeleteExpenseCommand, bool>
{
    private readonly IAppDbContext _db;

    public DeleteExpenseCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteExpenseCommand command, CancellationToken ct)
    {
        var entity = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == command.ExpenseId && x.UserId == command.UserId, ct);

        if (entity == null) throw new ArgumentException("Chi phí không tồn tại");

        _db.Expenses.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}