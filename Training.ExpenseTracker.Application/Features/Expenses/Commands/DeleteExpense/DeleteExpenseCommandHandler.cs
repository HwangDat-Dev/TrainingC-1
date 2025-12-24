using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandHandler
    : ICommandHandler<DeleteExpenseCommand, bool>
{
    private readonly IWriteDbContext _db;
    private readonly ILogger<DeleteExpenseCommandHandler> _logger;

    public DeleteExpenseCommandHandler(IWriteDbContext db, ILogger<DeleteExpenseCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteExpenseCommand command, CancellationToken ct)
    {
        _logger.LogInformation("[DB:WRITE] DeleteExpense: {ExpenseId}", command.ExpenseId);
        
        var entity = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == command.ExpenseId && x.UserId == command.UserId, ct);

        if (entity == null) throw new ArgumentException("Chi phí không tồn tại");

        _db.Expenses.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}