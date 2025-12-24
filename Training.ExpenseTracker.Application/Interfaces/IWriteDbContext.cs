using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Application.Interfaces;

public interface IWriteDbContext
{
    DbSet<User> Users { get; }
    DbSet<Expense> Expenses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
