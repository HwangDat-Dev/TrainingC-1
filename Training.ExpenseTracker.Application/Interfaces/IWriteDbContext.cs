using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Application.Interfaces;

public interface IWriteDbContext
{
    DbSet<Expense> Expenses { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}